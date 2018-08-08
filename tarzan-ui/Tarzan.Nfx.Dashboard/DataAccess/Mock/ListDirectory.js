var fs = require("fs");

function fsReaddirPromise(filePath) {
    return new Promise(function (resolve, reject) {
        fs.readdir(filePath, function (err, filePaths) {
            if (err) {
                reject(err);
                return;
            }
            resolve(filePaths);
        });
    });
};


fsReaddirPromise('.').then(function (files) {
    var i = 0;
    files.forEach(function (file) {
        fs.stat(file, function (err, stats) {
            console.log(' new Capture() { ');
            i = i + 1;
            console.log(' Id   = ' + i + ',');
            console.log(' Name = "' + file + '",');
            console.log(' Size = ' + stats.size + ',');
            console.log(' Type = "pcap",');
            console.log(' CreatedOn = DateTimeOffset.FromUnixTimeMilliseconds (' + Math.floor(stats.birthtimeMs) + ').DateTime,');
            console.log(' UploadOn = DateTimeOffset.FromUnixTimeMilliseconds (' + Math.floor(Date.now()) + ').DateTime,');
            console.log(' }, ');
        });
    });
}).then(function (result) {
    console.log(']');
});
