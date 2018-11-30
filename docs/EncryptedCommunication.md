# Detecting artifacts in Encrypted Communication
The TLS communication is modeled around the conversation entity, which is a bidirectional flow. 

## Features
The ```TlsConversation``` entity has the following fields:

| Field                 |  Type        | Description                    |
| --------------------- | ------------ | ------------------------------ |
| flowkey               | string       | |              
| tls_version           | string       | |
| session_id            | string       | A hexadecimal representation of the session id. |
| client_random         | string       | A hexadecimal representation of the client random string. |
| client_cipher_suites  | string[]     | The array of string representing offered cipher suites. |
| client_compressions   | string[]     | The array of offered compression methods. |
| client_certificates   | string[]     | Server side certificates canonical names. |
| client_extensions     | string[]     | List of extension parameters. The string has format 'name=value'. |
| server_random         | string       | |
| server_cipher_suite   | string       | The selected cipher suite. |
| server_compression    | string       | The selected compression method. |
| server_certificates   | string[]     | Server side certificates canonical names. |
| server_extensions     | string[]     | List of extension parameters. |
| records               | tls_record[] |  |


The ```TlsRecord``` entity has the following fields:

| Field                 |  Type         | Description                                              |
| --------------------- | ------------- | -------------------------------------------------------- |
| number                | int           | The number of the record in the TLS conversation.        |
| time_offset           | long          | Amount of ms since the beginning of the conversation.    |
| length                | int           | Length of the record.                                    |
| segments              | tcp_segment[] | An array of TCP segments carrying data for te=he record. |

The ```TcpSegment``` entity has the following fields:

| Field                 |  Type         | Description                                              |
| --------------------- | ------------- | -------------------------------------------------------- |
| number                | int           | Number of the TCP segment in the TLS conversation.       |
| time_offset           | long          | Amount of ms since the beginning of the conversation.    |
| length                | int           | Length of the TCP segment. |
| flags                 | string        | Tcp flags. |
| window                | int           | Tcp window size. |

## Notes

The TLS communication is carried in a TCP session. 
The TLS communication consists of initial handshake followed by data exchange.
The data TLS communication is represented by a series of TLS records. 
For HTTP the request is contained in a single TLS record, which may be split in 
several TCP segments.
 

The analysis of artifact secured in the encrypted communication is based on creating a behavioral model 
of the communication. The features for creating the model are selected from information that characterizes
the underlaying TCP communication:

* number of packets in the conversations
* total duration of the conversation
* number of packets with the TCP PUSH flag set
* average packet size for each direction
* inter-arrival time between packets

Featresd related to TLS communicaton:
* sizes of TLS records in bytes 
* 



In addition to these features, the TLS handshake messages provide some interesting information as plain text:

* Version
* Cipher suites
* Extensions
* 

## HTTP Artifact Identification


### Datasets
The method is based on finding similaritites of encrypted HTTPS traffic with the corresponding plain HTTP communication.
Thus to design the method, we need to have a dataset of HTTPS and corresponding HTTP messages.
To get the HTTP plain communication for secured HTTPS communication we use use key logging feature of the web browser [https://developer.mozilla.org/en-US/docs/Mozilla/Projects/NSS/Key_Log_Format]. Knowing master key enable to 
see HTTP in Wireshark, for example.




## References

* Anderson: Detecting Encrypted Malware Traffic (Without Decryption) [https://blogs.cisco.com/security/detecting-encrypted-malware-traffic-without-decryption]

* Anderson, Chi, Dunlop, McGrew: Limitless HTTP in an HTTPS World: Inferring the
Semantics of the HTTPS Protocol without Decryption [https://arxiv.org/pdf/1805.11544.pdf]