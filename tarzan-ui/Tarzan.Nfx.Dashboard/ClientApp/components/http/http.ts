import Vue from 'vue';
import { TableViewComponent } from '../templates/tableViewComponent';
import { Component } from 'vue-property-decorator';
import { HttpInfo } from '../../api/models';
import { FileSaver } from '../../utils/file-saver';
import { Base64 } from 'js-base64';
import { Buffer } from 'buffer';
@Component
export default class HttpComponent extends TableViewComponent<HttpInfo> {
    cachedItems: { [id: string]: HttpInfo; } = {};

    constructor() {
        super("api/http/");        
    }
    mounted() {
        this.fetch();
    }
    reload(page: number) {
        super.reload(page);
    }

    async getItem(flowId: string, transactionId: string) {
        let key = flowId + "-" + transactionId;
        let item = this.cachedItems[key];
        if (item == undefined) {
            let fetchString = `api/http/item/${flowId}/${transactionId}`;
            const res = await fetch(fetchString);
            const json = await res.json();
            return json as HttpInfo;
        }
        return this.cachedItems[key];
    }

    getBody(contentType: string | undefined, chunks: string[] | undefined): Buffer[] {
        if (chunks == undefined) {
            return [];
        }
        else {
            return chunks.map(content => new Buffer(content, 'base64'));
        }
    }

    lineWrap(content: string) {
        return content.replace(/(.{1,80})/g, '$1\r\n');
    }

    getRequestContent(flowId: string, transactionId: string) {
        this.getItem(flowId, transactionId).then(item => this.getBody(item.requestContentType, item.requestBodyChunks));
    }
      
    getResponseContent(flowId: string, transactionId: string) {
        this.getItem(flowId, transactionId).then(item => this.getBody(item.responseContentType, item.responseBodyChunks));
    }

    downloadContent(content: Buffer[], filename: string, mimeType: string) {
        let fileSaver = new FileSaver();
        fileSaver.responseData = content;
        fileSaver.strFileName = filename;
        fileSaver.strMimeType = mimeType;
        fileSaver.initSaveFile();
    }

    downloadResponseContent(flowId: string, transactionId: string) {
        this.getItem(flowId, transactionId).then(item => {
            if (item) {
                let chunks = item.responseBodyChunks ? item.responseBodyChunks : []; 
                let content = this.getBody(item.responseContentType, item.responseBodyChunks);
                let filename = item.uri ? item.uri : "file.raw";
                try {   
                    this.downloadContent(content, filename, "application/octet-stream");
                }
                catch (err) { console.log("ERROR: downloadResponseContent:" + err); }
                this.downloadContent(chunks.map(x=> new Buffer(x)), filename + ".base64", "application/octet-stream");
            }
        });
    }
}


