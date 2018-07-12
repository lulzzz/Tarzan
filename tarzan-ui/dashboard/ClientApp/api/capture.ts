/**
* Represents a single flow record.
*/
export class Capture {
    /**
    * A unique identifier of the capture file.
    */
    'id': number;
    /**
    * The name of the capture file..
    */
    'name': string;
    /**
    * A type of the capture file. It can be pcap, pcapng, etc.
    */
    'type': string;
    /**
    * The total size of the capture file.
    */
    'size': number;
    /**
    * Timestamp when the file was originally created.
    */
    'createdOn': Date;
    /**
    * Timestamp when the file was upload to the system.
    */
    'uploadOn': Date;
    /**
    * Hash value computed by MD5 algorithm.
    */
    'hash': string;
    /**
    * Name of the person who captured the file.
    */
    'author': string;
    /**
    * Arbitrary notes associated with the capture.
    */
    'notes': string;
    /**
    * List of tags that label the capture.
    */
    'tags': Array<string>;

    static discriminator = undefined;

    static attributeTypeMap: Array<{ name: string, baseName: string, type: string }> = [
        {
            "name": "id",
            "baseName": "id",
            "type": "number"
        },
        {
            "name": "name",
            "baseName": "name",
            "type": "string"
        },
        {
            "name": "type",
            "baseName": "type",
            "type": "string"
        },
        {
            "name": "size",
            "baseName": "size",
            "type": "number"
        },
        {
            "name": "createdOn",
            "baseName": "createdOn",
            "type": "Date"
        },
        {
            "name": "uploadOn",
            "baseName": "uploadOn",
            "type": "Date"
        },
        {
            "name": "hash",
            "baseName": "hash",
            "type": "string"
        },
        {
            "name": "author",
            "baseName": "author",
            "type": "string"
        },
        {
            "name": "notes",
            "baseName": "notes",
            "type": "string"
        },
        {
            "name": "tags",
            "baseName": "tags",
            "type": "Array<string>"
        }];

    static getAttributeTypeMap() {
        return Capture.attributeTypeMap;
    }
}
