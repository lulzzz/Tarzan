# Detecting artifacts in Encrypted Communication


The analysis of artifact secured in the encrypted communication is based on creating a behavioral model 
of the communication. The features for creating the model are selected from information that characterizes
the communication:
* packet lengths
* inter-arrival time between packets
* ...

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