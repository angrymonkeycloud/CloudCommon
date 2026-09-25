import {readFile,writeFile,readdir,mkdir,copyFile,access} from 'node:fs/promises';
import path from 'node:path';import {execFileSync} from 'node:child_process';import less from 'less';
const common=path.resolve(import.meta.dirname,'../..'),workspace=path.resolve(common,'..'),cdm=path.resolve(workspace,'../CDM');
const ignore=new Set(['.git','.vs','bin','obj','node_modules','artifacts','NugetPackage']);
async function find(dir){let out=[];for(const e of await readdir(dir,{withFileTypes:true})){if(ignore.has(e.name))continue;const file=path.join(dir,e.name);if(e.isDirectory())out.push(...await find(file));else if(e.name==='mateconfig.json'||e.name.endsWith('.razor.less'))out.push(file);}return out;}
const targets=new Map();const roots=['CloudComponents','CloudLogin','CloudCommerce','CloudBlazor'].map(p=>path.join(workspace,p)).concat(cdm);
for(const root of roots)for(const file of await find(root)){
 if(file.endsWith('.razor.less'))targets.set(file.slice(0,-4)+'css',{input:file,output:file.slice(0,-4)+'css'});
 else {const config=JSON.parse((await readFile(file,'utf8')).replace(/^\uFEFF/,''));for(const entry of config.files??[]){const inputs=(Array.isArray(entry.input)?entry.input:[entry.input]).filter(i=>typeof i==='string'&&!/https?:/.test(i)).map(i=>path.resolve(path.dirname(file),i));for(const out of (Array.isArray(entry.output)?entry.output:[entry.output])){if(typeof out!=='string'||!inputs.length||!inputs.every(i=>/\.(less|js)$/.test(i)))continue;const output=path.resolve(path.dirname(file),out);targets.set(output,{input:inputs[0],inputs,output});}}}
}
const errors=[];let compiled=0,copied=0;
for(const {input,inputs=[input],output}of targets.values()){
 try{const source=inputs.length===1?(await readFile(input,'utf8')).replace(/^\uFEFF/,''):input.endsWith('.less')?inputs.map(i=>'@import "'+i.replaceAll('\\','/')+'";').join('\n'):(await Promise.all(inputs.map(i=>readFile(i,'utf8')))).join('\n;\n');await mkdir(path.dirname(output),{recursive:true});
 if(input.endsWith('.less')){const result=await less.render(source,{filename:input,math:'parens-division',rewriteUrls:'all'});await writeFile(output,'/* Generated from LESS. Do not edit. */\n'+result.css);const min=output.replace(/\.css$/,'.min.css');let exists=false;try{await access(min);exists=true;}catch{}if(exists){const compact=await less.render(source,{filename:input,math:'parens-division',rewriteUrls:'all',compress:true});await writeFile(min,compact.css);}compiled++;}
 else if(input!==output){await writeFile(output,source);copied++;}
 }catch(e){errors.push({file:path.relative(workspace,input),message:e.message,line:e.line});}
}
await mkdir(path.join(common,'artifacts'),{recursive:true});await writeFile(path.join(common,'artifacts/related-assets.json'),JSON.stringify({compiled,copied,errors},null,2));
console.log(JSON.stringify({compiled,copied,errors},null,2));if(errors.length)process.exitCode=1;
