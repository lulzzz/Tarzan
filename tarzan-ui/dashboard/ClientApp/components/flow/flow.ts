import Vue from 'vue';
import { Component } from 'vue-property-decorator';

@Component
export default class FlowComponent extends Vue {
    loading: boolean = true;
    currentFlow: string = '0';

    mounted() {
        this.currentFlow = this.$route.params.id;
    }
}


