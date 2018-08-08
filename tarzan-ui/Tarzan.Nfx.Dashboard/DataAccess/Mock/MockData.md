


# Captures

fs.readdir('.', function(err, files) {
    new Promise(function(resolve, reject){
        console.log('[');
        var i = 0;
        files.forEach(function(file) {
            fs.stat(file, function(err, stats) {
            console.log(' { ');
            i = i + 1;
            console.log(' "Id" : ' + i  + ',' );
            console.log(' "Name" : ' + file + ',');
            console.log(' "Size" : ' + stats.size + ',');
            console.log(' "Type" : "pcap",' );
            console.log(' "CreatedOn:" ' + stats.birthtimeMs + ',');
            console.log(' "UploadOn:" ' + Date.now() + ',');
            console.log(' }, ');
            });
        });
    }.then(function (result){
    console.log(']');
    });
});