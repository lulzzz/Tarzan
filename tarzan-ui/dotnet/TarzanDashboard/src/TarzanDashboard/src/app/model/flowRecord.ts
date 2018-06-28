export class FlowRecord
{
    id: number;
    protocol: string;
    sourceAddress : string;
    sourcePort: number;
    destinationAddress: string;
    destinationPort : number;
    firstSeen: string;
    lastSeen: string;
    octets: number;
    packets: number;
    notes:string;
    tags: string[];
}