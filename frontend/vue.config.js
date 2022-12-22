const fs = require('fs')
const path = require('path')

const baseFolder =
    process.env.APPDATA !== undefined && process.env.APPDATA !== ''
        ? `${process.env.APPDATA}/ASP.NET/http`
        : `${process.env.HOME}/.aspnet/http`;

//const certificateArg = process.argv.map(arg => arg.match(/--name=(?<value>.+)/i)).filter(Boolean)[0];
//const certificateName = certificateArg ? certificateArg.groups.value : "ParallelDB";

//if (!certificateName) {
//    console.error('Invalid certificate name. Run this script in the context of an npm/yarn script or pass --name=<<app>> explicitly.')
//    process.exit(-1);
//}

// const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
// const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

module.exports = {
    configureWebpack: {
        resolve: {
            alias: {
                "path": path.resolve(__dirname, 'null-alias.js'),
                "fs": path.resolve(__dirname, 'null-alias.js'),
                "process": path.resolve(__dirname, 'null-alias.js'),
            }
        }
    },
    devServer: {
        // https: {
        //     key: fs.readFileSync(keyFilePath),
        //     cert: fs.readFileSync(certFilePath),
        // },
        proxy: {
            '^/weatherforecast': {
                target: 'http://localhost:5002/'
            }
        },
        port: 5002
    }
}