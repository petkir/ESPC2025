const path = require('node:path');
const fs = require('node:fs');

function _parseEnvFile(filePath) {
  if (!fs.existsSync(filePath)) {
    return {};
  }

  // Lazy-load to avoid hard failure if someone hasn't run npm install yet.
  // Heft/webpack will still work without .env support.
  let dotenv;
  try {
    dotenv = require('dotenv');
  } catch (e) {
    return {};
  }

  try {
    const raw = fs.readFileSync(filePath);
    return dotenv.parse(raw);
  } catch (e) {
    return {};
  }
}

module.exports = function customizeWebpackConfig(webpackConfig, taskSession, heftConfiguration, webpack) {
  const projectRoot = heftConfiguration.buildFolderPath || path.join(__dirname, '..');

  // Merge .env then .env.local (local overrides base), but never override real environment variables.
  const envBase = _parseEnvFile(path.join(projectRoot, '.env'));
  const envLocal = _parseEnvFile(path.join(projectRoot, '.env.local'));
  const mergedEnv = { ...envBase, ...envLocal };
  for (const [key, value] of Object.entries(mergedEnv)) {
    if (!(key in process.env)) {
      process.env[key] = value;
    }
  }

  const get = (name) => (process.env[name] || '').toString();

  const definitions = {
    CUSTOMERWP_SECTORS_LIST_TITLE: JSON.stringify(get('CUSTOMERWP_SECTORS_LIST_TITLE')),
    CUSTOMERWP_REQUESTS_LIST_TITLE: JSON.stringify(get('CUSTOMERWP_REQUESTS_LIST_TITLE')),
    CUSTOMERWP_USE_MOCK_DATA: JSON.stringify(get('CUSTOMERWP_USE_MOCK_DATA')),
    CUSTOMERWP_APPINSIGHTS_CONNECTION_STRING: JSON.stringify(get('CUSTOMERWP_APPINSIGHTS_CONNECTION_STRING'))
  };

  webpackConfig.plugins = webpackConfig.plugins || [];
  webpackConfig.plugins.push(new webpack.DefinePlugin(definitions));
};
