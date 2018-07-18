import Vue from 'vue';
import { Component } from 'vue-property-decorator';

@Component
export default class FilesComponent extends Vue {
    fileSizes = [
        { value: 'tiny', label: '0-50MB' },
        { value: 'small', label: '50-200MB' },
        { value: 'large', label: '200MB-1GB' },
        { value: 'huge', label: '1GB+' },
    ]
    fileSizeFilter: string = "all";
}


