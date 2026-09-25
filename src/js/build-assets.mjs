import { readFile, writeFile, mkdir } from 'node:fs/promises';
import less from 'less';
const root = new URL('../../', import.meta.url);
for (const [input, output] of [
  ['CloudCommon.Theming/src/css/elements.less', 'CloudCommon.Theming/wwwroot/css/elements.css'],
  ['CloudCommon.Demo/src/css/workshop.less', 'CloudCommon.Demo/wwwroot/css/workshop.css']
]) {
  const result = await less.render(await readFile(new URL(input, root), 'utf8'), { filename: new URL(input, root).pathname });
  await mkdir(new URL('./', new URL(output, root)), { recursive: true });
  await writeFile(new URL(output, root), result.css);
}

