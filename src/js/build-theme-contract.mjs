import {readFile,writeFile,mkdir} from 'node:fs/promises';import {execFileSync} from 'node:child_process';import path from 'node:path';
const root=path.resolve(import.meta.dirname,'../..');
execFileSync('dotnet',['run','--project','CloudCommon.AssetBuilder','--','artifacts/theme-assets'],{cwd:root,stdio:'inherit'});
const values=JSON.parse(await readFile(path.join(root,'artifacts/theme-assets/theme-defaults.json'),'utf8'));
function concrete(key){return values[key].replace(/var\(--amc-([\w-]+)\)/g,(_,ref)=>concrete(ref));}
await writeFile(path.join(root,'CloudCommon.Theming/src/css/defaults.less'),'// Generated from ThemeResolver by build:contract. Do not edit.\n'+Object.keys(values).map(key=>'@'+key+': ~"'+concrete(key).replaceAll('"','\\"')+'";').join('\n')+'\n');
await mkdir(path.join(root,'CloudCommon.Theming.Blazor/wwwroot/css'),{recursive:true});
await writeFile(path.join(root,'CloudCommon.Theming.Blazor/wwwroot/css/theme.css'),await readFile(path.join(root,'artifacts/theme-assets/theme.css')));
console.log('Generated default CSS and LESS fallbacks from ThemeResolver.');
