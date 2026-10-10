const fs = require('fs');
const p = 'd:/Charity/charity-app/src/app/';
const dirs = ['beneficiaries', 'dashboard', 'distributions', 'donations', 'inventory', 'login', 'settings', 'staff'];

dirs.forEach(d => {
  const f = p + d + '/' + d + '.html';
  if (fs.existsSync(f)) {
    let html = fs.readFileSync(f, 'utf8');
    if (!html.includes('overflow-x: auto')) {
      html = html.replace(/<table/g, '<div style="overflow-x: auto; width: 100%; border-radius: 8px;"><table');
      html = html.replace(/<\/table>/g, '</table></div>');
      fs.writeFileSync(f, html);
    }
  }
});

const loginPath = p + 'login/login.ts';
if (fs.existsSync(loginPath)) {
  let login = fs.readFileSync(loginPath, 'utf8');
  login = login.replace('width: 350px;', 'width: 100%; max-width: 350px; box-sizing: border-box; margin: 15px;');
  fs.writeFileSync(loginPath, login);
}
