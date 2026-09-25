import {chromium} from 'playwright';
import assert from 'node:assert/strict';
import {readFile,mkdir,writeFile} from 'node:fs/promises';
import path from 'node:path';
const root=path.resolve(import.meta.dirname,'../..'),workspace=path.resolve(root,'..'),cdm=path.resolve(workspace,'../CDM');
const cdmFiles=['CDM.Components/wwwroot/css/site.css','CDM.Server/Layout/MainLayout.razor.css','CDM.Components/Pages/PortalSidebar.razor.css','CDM.Components/Shared/OptionButtons.razor.css','CDM.Components/Grid/Grid.razor.css'];
const loginFiles=['CloudLogin.Components/Components/Login/LoginComponent.razor.css','CloudLogin.Components/Components/Login/LoginHeaderComponent.razor.css','CloudLogin.Components/Components/Login/MainInput.razor.css'];
const gridFiles=['CloudComponents.DataGrid/Components/CloudDataGrid.razor.css','CloudComponents.DataGrid/Components/CloudDataGridBody.razor.css'];
async function css(repo,files){return (await Promise.all(files.map(file=>readFile(path.join(repo,file),'utf8')))).join('\n').replaceAll('::deep','').replace(/@import[^;]*;/g,'');}
const browser=await chromium.launch({channel:'msedge',headless:true});
const page=await browser.newPage({viewport:{width:1280,height:800}});
const properties=['fontSize','fontWeight','paddingTop','paddingRight','paddingBottom','paddingLeft','borderRadius','borderTopWidth','minHeight','lineHeight','display'];
const inspect=selector=>page.locator(selector).first().evaluate((e,props)=>{const s=getComputedStyle(e);return Object.fromEntries(props.map(p=>[p,s[p]]));},properties);
const transparent=selector=>page.locator(selector).first().evaluate(e=>getComputedStyle(e).backgroundColor==='rgba(0, 0, 0, 0)');
const html='<body class="cdm-shell"><aside class="portal-sidebar"><div class="portal-sidebar-brand">Portal</div><div class="portal-sidebar-body"><nav>'+Array.from({length:100},(_,i)=>'<p>Navigation '+i+'</p>').join('')+'</nav></div></aside><main class="portal-content"><div class="view-tabs"><button id="tab">Records</button></div><div id="buttons">'+['','_solid','_ghost','_danger','_secondary'].map((m,i)=>'<button id="button'+i+'" class="--cdm-button '+m+'">Action</button>').join('')+'</div><div class="option-buttons"><button class="option-buttons-button">Open</button><button class="option-buttons-button _selected">Selected</button><button class="option-buttons-button _disabled" disabled>Disabled</button></div><input id="native-checkbox" type="checkbox"><div id="long-form">'+Array.from({length:100},(_,i)=>'<p>Form field '+i+'</p>').join('')+'</div></main></body>';
const checks=[];
try{
const selectors=['#button0','#button1','#button2','#button3','#button4','#tab','.option-buttons-button'];
const baseline=JSON.parse(await readFile(path.join(root,'tests/browser/appearance-baseline.json'),'utf8'));
const original=baseline.cdm;
const currentCdm=await css(cdm,cdmFiles,false);
await page.setContent('<style>'+currentCdm+'</style>'+html);
for(const s of selectors)assert.deepEqual(await inspect(s),original[s],'Original CDM geometry '+s);
assert.equal(await transparent('#button0'),true);
assert.equal(await transparent('#button1'),false);
assert.equal(await transparent('#button2'),true);
assert.equal(await transparent('#button3'),true);
assert.equal(await transparent('#button4'),false);
assert.equal(await transparent('#tab'),true);
assert.equal(await transparent('.option-buttons-button'),true);
await page.locator('#button2').hover();await page.waitForTimeout(250);assert.equal(await transparent('#button2'),false);
await page.mouse.move(1200,780);
checks.push('CDM original button, tab and option geometry; solid/ghost/danger/secondary fills');
for(const width of [1280,390]){
await page.setViewportSize({width,height:800});
await page.locator('main').hover();await page.mouse.wheel(0,600);await page.waitForTimeout(150);
assert.ok(await page.locator('main').evaluate(e=>e.scrollTop>0),'Long form scrolls at '+width);
assert.ok(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth+1),'No horizontal document overflow at '+width);
if(width>760){await page.locator('.portal-sidebar nav').hover();await page.mouse.wheel(0,400);await page.waitForTimeout(150);assert.ok(await page.locator('.portal-sidebar nav').evaluate(e=>e.scrollTop>0),'Sidebar scrolls independently');}
}
checks.push('Desktop/mobile long-form wheel scrolling and independent sidebar scrolling');
await page.setViewportSize({width:1280,height:800});
const gridCss=await css(path.join(workspace,'CloudComponents'),gridFiles,false);
const grid='<body class="cdm-shell"><main class="portal-content"><div class="viewpage"><div class="cdm-view-host"><div class="cdm-grid-host"><div class="cloudgrid _full-height"><div class="cloudgrid-table"><div class="cloudgrid-body">'+Array.from({length:150},(_,i)=>'<div class="cloudgrid-row"><div class="cloudgrid-rowmain">Record '+i+'</div></div>').join('')+'</div></div></div></div></div></div></main></body>';
await page.setContent('<style>'+currentCdm+gridCss+'</style>'+grid);
const scrollable=await page.locator('.cloudgrid-table').evaluate(e=>({client:e.clientHeight,scroll:e.scrollHeight,overflow:getComputedStyle(e).overflowY}));
assert.ok(scrollable.scroll>scrollable.client,'Grid content exceeds bounded viewport');
await page.locator('.cloudgrid-table').hover();await page.mouse.wheel(0,700);await page.waitForTimeout(150);assert.ok(await page.locator('.cloudgrid-table').evaluate(e=>e.scrollTop>0),'Full-height grid wheel scrolls');
checks.push('Full-height grid has a bounded, wheel-scrollable viewport');
const loginHtml='<body><div class="amc-cloudlogin"><h1>Sign in</h1><div class="input"><label class="input-label">Email</label><div class="input-content"><input placeholder="Enter email"></div></div><button class="amc-cloudlogin-provider">Create new account</button><button class="amc-cloudlogin-button">Next</button></div></body>';
const loginSelectors=['.amc-cloudlogin','.amc-cloudlogin-provider','.amc-cloudlogin-button','.input-content input','.input-label'];
const loginOriginal=baseline.login;
await page.setContent('<style>'+await css(path.join(workspace,'CloudLogin'),loginFiles,false)+'</style>'+loginHtml);
for(const s of loginSelectors)assert.deepEqual(await inspect(s),loginOriginal[s],'Original CloudLogin geometry '+s);
assert.equal(await page.locator('.amc-cloudlogin-provider').evaluate(e=>getComputedStyle(e).backgroundColor),'rgb(255, 255, 255)');
await page.locator('.input-content input').focus();assert.equal(await page.locator('input').evaluate(e=>getComputedStyle(e).outlineStyle),'solid');
checks.push('CloudLogin original card, provider, action and input geometry; keyboard focus');
await mkdir(path.join(root,'artifacts/appearance-repair'),{recursive:true});
await page.screenshot({path:path.join(root,'artifacts/appearance-repair/login-comparison.png')});
await writeFile(path.join(root,'artifacts/appearance-repair/browser-results.json'),JSON.stringify({passed:true,checks},null,2));
console.log(JSON.stringify({passed:true,checks},null,2));
}finally{await browser.close();}
