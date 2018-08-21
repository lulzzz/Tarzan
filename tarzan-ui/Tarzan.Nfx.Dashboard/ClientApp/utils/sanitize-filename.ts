
export class FileSanitizer {
    illegalRe = /[\/\?<>\\:\*\|":]/g;
    controlRe = /[\x00-\x1f\x80-\x9f]/g;
    reservedRe = /^\.+$/;
    windowsReservedRe = /^(con|prn|aux|nul|com[0-9]|lpt[0-9])(\..*)?$/i;
    windowsTrailingRe = /[\. ]+$/;


    isHighSurrogate(codePoint:number) {
        return codePoint >= 0xd800 && codePoint <= 0xdbff;
    }

    isLowSurrogate(codePoint:number) {
        return codePoint >= 0xdc00 && codePoint <= 0xdfff;
    }
    byteLength(str:string) {
    // returns the byte length of an utf8 string
    var s = str.length;
    for (var i = str.length - 1; i >= 0; i--) {
        var code = str.charCodeAt(i);
        if (code > 0x7f && code <= 0x7ff) s++;
        else if (code > 0x7ff && code <= 0xffff) s += 2;
        if (code >= 0xDC00 && code <= 0xDFFF) i--; //trail surrogate
    }
    return s;
    }

    truncate(string: string, byteLength: number) {
        if (typeof string !== "string") {
            throw new Error("Input must be string");
        }

        var charLength = string.length;
        var curByteLength = 0;
        var codePoint;
        var segment;

        for (var i = 0; i < charLength; i += 1) {
            codePoint = string.charCodeAt(i);
            segment = string[i];

            if (this.isHighSurrogate(codePoint) && this.isLowSurrogate(string.charCodeAt(i + 1))) {
                i += 1;
                segment += string[i];
            }

            curByteLength += this.byteLength(segment);

            if (curByteLength === byteLength) {
                return string.slice(0, i + 1);
            }
            else if (curByteLength > byteLength) {
                return string.slice(0, i - segment.length + 1);
            }
        }
    }

    clean(input :string | undefined, replacement:any) {
        var sanitized = (input ? input : "")
            .replace(this.illegalRe, replacement)
            .replace(this.controlRe, replacement)
            .replace(this.reservedRe, replacement)
            .replace(this.windowsReservedRe, replacement)
            .replace(this.windowsTrailingRe, replacement);
        return this.truncate(sanitized, 255);
    }

    sanitize(input: string | undefined, options: any) : string {
        if (input == undefined) return "";
        var replacement = (options && options.replacement) || '';
        var output = this.clean(input, replacement);
        if (replacement === '') {
            return output ? output : "";
        }
        var output2 = this.clean(output, '');
        return output2 ? output2 : "";
    }
}