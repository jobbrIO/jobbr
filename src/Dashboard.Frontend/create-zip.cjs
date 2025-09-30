const fs = require('fs');
const path = require('path');
const archiver = require('archiver');

const distPath = path.join(__dirname, 'dist');
const outputPath = path.join(__dirname, '..', 'Dashboard', 'dashboard-app.zip');

console.log('Creating dashboard-app.zip from dist folder...');

const targetDir = path.dirname(outputPath);
if (!fs.existsSync(targetDir)) {
  console.error('The target directory is missing. This script should only be run within the Jobbr repository.');
  process.exit(1);
}

const output = fs.createWriteStream(outputPath);
const archive = archiver('zip', {
  zlib: { level: 9 }
});

// Listen for all archive data to be written
output.on('close', function() {
  console.log(`dashboard-app.zip successfully created (${archive.pointer()} total bytes)`);
});

archive.on('error', function(err) {
  console.error('Error creating zip:', err);
  process.exit(1);
});

// Connect the archive to the output file
archive.pipe(output);

// Add the dist directory contents
if (fs.existsSync(distPath)) {
  archive.directory(distPath, false);
} else {
  console.error('dist/ directory does not exist. Ensure the Vite build was successful.');
  process.exit(1);
}

archive.finalize();