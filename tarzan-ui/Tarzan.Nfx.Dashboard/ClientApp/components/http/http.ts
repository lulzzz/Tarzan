import Vue from 'vue';
import { TableViewComponent } from '../templates/tableViewComponent';
import { Component } from 'vue-property-decorator';
import { HttpInfo } from '../../api/models';

@Component
export default class HttpComponent extends TableViewComponent<HttpInfo> {
    constructor() {
        super("api/http/");        
    }
    mounted() {
        this.fetch();
    }
    reload(page: number) {
        super.reload(page);
    }
}


