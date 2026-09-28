import assert from 'node:assert/strict';
import {readFile} from 'node:fs/promises';
import path from 'node:path';
import {chromium} from 'playwright';

const workspace = path.resolve(import.meta.dirname, '../../..');
const styles = await Promise.all([
  'CloudCommon/CloudCommon.Theming/wwwroot/css/elements.css',
  'CloudLogin/CloudLogin.WebAssembly/wwwroot/css/site.css',
  'CloudLogin/CloudLogin.Components/Components/Login/LoginComponent.razor.css',
].map(file => readFile(path.join(workspace, file), 'utf8')));

const browser = await chromium.launch({channel: 'msedge', headless: true});
try {
  const page = await browser.newPage();
  await page.setContent(`<style>${styles.join('\n')}
    :root { --amc-button-active-background: #123456; --amc-button-active-color: #fafafa; --cloudlogin-button-active-background: #393939; }
    .new-button { background: white; color: #111; }
    .new-button:hover { background: #f5f5f5; color: #111; }
    </style>
    <button id="plain">Plain</button>
    <button class="new-button" data-cloud-element="button">Themed</button>
    <div class="amc-cloudlogin">
      <button id="provider" class="amc-cloudlogin-provider _google">Google</button>
      <button id="action" class="amc-cloudlogin-button _solid">Sign in</button>
    </div>`);

  const background = selector => page.locator(selector).evaluate(element => getComputedStyle(element).backgroundColor);
  const color = selector => page.locator(selector).evaluate(element => getComputedStyle(element).color);
  const press = async selector => {
    await page.locator(selector).hover();
    await page.waitForTimeout(150);
    const hover = await background(selector);
    const hoverColor = await color(selector);
    await page.mouse.down();
    await page.waitForTimeout(150);
    assert.equal(await background(selector), hover, `${selector} background changes on press`);
    assert.equal(await color(selector), hoverColor, `${selector} text changes on press`);
    await page.mouse.up();
  };

  assert.equal(await background('#provider'), 'rgb(255, 255, 255)');
  await press('#provider');
  await press('#action');
  await press('.new-button');

  await page.locator('#provider').evaluate(element => element.setAttribute('aria-pressed', 'true'));
  assert.equal(await background('#provider'), 'rgb(255, 255, 255)', 'Provider selected state keeps new surface');
  await page.locator('#provider').evaluate(element => element.removeAttribute('aria-pressed'));
  await page.mouse.move(0, 0);
  assert.equal(await background('#provider'), 'rgb(255, 255, 255)', 'Provider returns to new normal style');

  await page.locator('#plain').hover();
  await page.mouse.down();
  await page.waitForTimeout(200);
  assert.equal(await background('#plain'), 'rgb(57, 57, 57)', 'Plain button retains shared active recipe');
  await page.mouse.up();

  await page.locator('#plain').focus();
  await page.keyboard.press('Tab');
  await page.keyboard.press('Tab');
  assert.equal(await page.evaluate(() => document.activeElement.id), 'provider');
  assert.equal(await background('#provider'), 'rgb(255, 255, 255)', 'Keyboard focus keeps provider surface');
  assert.equal(await page.locator('#provider').evaluate(element => getComputedStyle(element).outlineStyle), 'solid');
  console.log('Button normal, hover, press, selected and keyboard-focus styles pass.');
} finally {
  await browser.close();
}
