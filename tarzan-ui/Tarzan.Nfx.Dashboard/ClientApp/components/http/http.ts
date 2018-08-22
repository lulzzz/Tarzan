import Vue from 'vue';
import { TableViewComponent } from '../templates/tableViewComponent';
import { Component } from 'vue-property-decorator';
import { HttpInfo } from '../../api/models';
import { FileSaver } from '../../utils/file-saver';
import { Base64 } from 'js-base64';
import { Buffer } from 'buffer';
import { stringify } from 'query-string';

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
        let key = flowId + "/" + transactionId;
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
        let buffer = Buffer.concat(content)
        fileSaver.responseData = buffer; 
        fileSaver.strFileName = filename;
        fileSaver.strMimeType = mimeType;
        fileSaver.initSaveFile();
    }

    downloadResponseContent(flowId: string, transactionId: string) {
        console.log("Downloading " + flowId + "/" + transactionId);
        this.getItem(flowId, transactionId).then(item => {
            if (item) {
                console.log("Getting " + item.flowId + "/" + item.transactionId);
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


    // FILTERING
    filterUri = "";
    filterContentType: string[] = [];
    contentTypeOptions = [{
        value: 'text/html',
        label: 'HTML'
        }, {
        value: 'text/css',
        label: 'CSS'
        }, {
        value: 'text/javascript',
        label: 'JavaScript'
        }, {
            value: 'image/jpeg',
            label: 'JPEG'
        }, {
            value: 'image/gif',
            label: 'GIF'
        }, {
            value: 'image/png',
            label: 'PNG'
        }, {
            value: 'application/octet-stream',
            label: 'OctetStream'
        }, {
            value: 'application/msword',
            label: 'Microsoft Word'
        }
    ];
    filterAtLeastValue = "";
    filterAtLeastUnit = "KB";
    filterAtMostValue = "";
    filterAtMostUnit = "KB";
    filterDateTimeRange = "";
    filterText = "All HTTP objects.";
    filterPopoverVisible = false;

    setFilter() {           
        this.queryParams = "?" + stringify({
            uri: this.filterUri,
            contentTypeList: this.filterContentType,
            atLeastSize: this.filterAtLeastValue.toString() + this.filterAtLeastUnit,
            atMostSize: this.filterAtMostValue.toString() + this.filterAtMostUnit,
            timeRange: this.filterDateTimeRange
        });

        let filterTextArray = [];
        if (this.filterUri.trim().length > 0) {
            filterTextArray.push(`URI matches "${this.filterUri.trim()}"`);
        }
        if (this.filterContentType.length > 0) {
            filterTextArray.push(`Content-Type is one of { ${this.filterContentType} }`);
        }

        if (this.filterAtLeastValue.length > 0) {
            filterTextArray.push(`Content-Length is at least ${this.filterAtLeastValue} ${this.filterAtLeastUnit}`);
        }

        if (this.filterAtMostValue.length > 0) {
            filterTextArray.push(`Content-Length is at most ${this.filterAtMostValue} ${this.filterAtMostUnit}`);
        }

        this.filterText = filterTextArray.join(" and ") + "."; 
        this.filterPopoverVisible = false;
        console.log("SET-FILTER:" + this.queryParams);        
        this.fetch();
    }

    resetFilter() {
        this.filterUri = "";
        this.filterContentType = [];
        this.filterAtLeastValue = "";
        this.filterAtLeastUnit = "KB";
        this.filterAtMostValue = "";
        this.filterAtMostUnit = "KB";
        this.filterDateTimeRange = "";
        this.filterText = "All HTTP objects."
        this.filterPopoverVisible = false;
        this.queryParams = "";
        console.log("RESET-FILTER");        
        this.fetch();
    }
}


