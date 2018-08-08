import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { Flow } from '../../api/flow';

@Component
export default class FlowComponent extends Vue {
    loading: boolean = true;
    flowRecord: Flow = new Flow();
    newTagInputValue = '';
    newTagInputVisible = false;

    mounted() {
        if (this.$route.params.id) {
            this.loadRecord(this.$route.params.id);
        }        
    }
    loadRecord(flowId: string) {
        let fetchString = `api/flows/item/${flowId}`;   
        console.log(fetchString);
        fetch(fetchString)
            .then(response => response.json() as Promise<Flow>)
            .then(data => {
                this.flowRecord = data;
                this.loading = false;
            });  
    }

    onUpdateFlow() {
        console.log('onUpdateFlow');
    }

    /*
     * Support for TAGs
     */
    tagHandleClose(tag: string) {
        console.log('handleClose');
        this.flowRecord.tags = this.flowRecord.tags.splice(this.flowRecord.tags.indexOf(tag), 1);
    }

    tagHandleInputConfirm(event:any) {
        event.preventDefault();
        console.log('handleInputConfirm');
        let inputValue = this.newTagInputValue;
        if (inputValue) {
            if (this.flowRecord.tags == null) {
                this.flowRecord.tags = new Array<string>();       
            }
            this.flowRecord.tags.push(inputValue);
        }
        this.newTagInputVisible = false;
        this.newTagInputValue = '';
    }

    tagShowInput() {
        console.log('showInput');
        this.newTagInputVisible = true;
        //todo: set focus to input field ~ following has type error:
        // this.$nextTick().then(_ => this.$refs.saveTagInput.$refs.input.focus());        
    }
    // END
}


