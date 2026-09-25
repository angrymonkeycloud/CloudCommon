import {readFile,readdir} from 'node:fs/promises';import path from 'node:path';import assert from 'node:assert/strict';
const workspace=path.resolve(import.meta.dirname,'../../..');
const projects=[
'CloudComponents/CloudComponents','CloudComponents/CloudComponents.DataGrid','CloudComponents/CloudComponents.TextEditor','CloudComponents/CloudComponents.Maps','CloudComponents/CloudComponents.VideoPlayer',
'CloudLogin/CloudLogin.Components','CloudLogin/CloudLogin.WebAssembly','CloudCommerce/CloudCommerce.Components',
'../CDM/CDM.Components','../CDM/CDM.Server.WebAssembly','../CDM/CDM.Server'
];
async function walk(dir){const files=[];for(const item of await readdir(dir,{withFileTypes:true})){if(['bin','obj','node_modules','.git','wwwroot'].includes(item.name))continue;const file=path.join(dir,item.name);if(item.isDirectory())files.push(...await walk(file));else if(file.endsWith('.less'))files.push(file);}return files;}
const violations=[];let count=0;
for(const project of projects){const root=path.resolve(workspace,project);for(const file of await walk(root)){count++;if(path.basename(file)==='theme.less')continue;const source=(await readFile(file,'utf8')).replace(/\/\*[\s\S]*?\*\//g,'').replace(/^\s*\/\/.*$/gm,'');if(/var\(\s*--amc-|\.amc-control\s*\(/.test(source))violations.push(path.relative(root,file));}}
assert.deepEqual(violations,[],'Use project-local theme variables and mixins instead of generic CloudTheme references.');
console.log('Verified '+count+' LESS files use local theme contracts.');

