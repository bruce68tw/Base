const gulp = require('gulp');
const concat = require('gulp-concat');
const cleanCSS = require('gulp-clean-css');
const uglify = require('gulp-uglify');
const rename = require('gulp-rename');
const fs = require('fs');

// 讀取 bundleconfig.json
const bundleConfig = JSON.parse(fs.readFileSync('./bundleconfig.json', 'utf8'));

/**
 * 處理打包與壓縮的通用函數
 */
function processBundle(bundle) {
    const isJs = bundle.outputFileName.endsWith('.js');
    const isCss = bundle.outputFileName.endsWith('.css');
    const fileName = bundle.outputFileName.split('/').pop();
    const destDir = bundle.outputFileName.substring(0, bundle.outputFileName.lastIndexOf('/'));

    return gulp.src(bundle.inputFiles)
        .pipe(concat(fileName))
        .pipe(gulp.dest(destDir)) // 輸出原始版 (例如: lib.js)
        .pipe(isJs ? uglify() : cleanCSS()) // 根據類型壓縮
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest(destDir)); // 輸出壓縮版 (例如: lib.min.js)
}

// 動態產生任務
const bundleTasks = bundleConfig.map(bundle => {
    const taskName = `bundle:${bundle.outputFileName}`;
    gulp.task(taskName, () => processBundle(bundle));
    return taskName;
});

gulp.task('default', gulp.parallel(...bundleTasks));