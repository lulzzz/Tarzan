# Encrypted Traffic
Prediction suggests that over 80% of network communication will be encrypted by the end of 2019. This makes  
systems that perform traffic analysis based on the protocol decoding obsolete. The same holds for the  
network forensics tools as most of them aim at providing extraction of the communication content, which cannot be  
done in case of secured data transfer.  

In order to analyze the network communication and extract meaningful information from the secured communication  
it is necessary to use methods that can cope with the secured traffic. It will not be possible to obtain the plain content,  
but still it may be possible to derive enough information for detecting security threats or forensic artifacts.  

# Classification Techniques



## Analysis of TLS Handshake Messages
TLS Handshake is a source of fruitful information. For instance:

* SSL/TLS certificate provide a name of the server which often corresponds to the service. Up to TLSv1.2, certificates are send before the 
parties start encrypting the traffic and thus can be easily analyzed.

* Server Name Indication (SNI) Extension is usually sent by the client to identify the correct server in case of server virtualization. 
  For instance, the following shows the fragment in TLSv1.3 ClientHello message:

```
Extension: server_name (len=20)
    Type: server_name (0)
    Length: 20
    Server Name Indication extension
        Server Name list length: 18
        Server Name Type: host_name (0)
        Server Name length: 15
        Server Name: mail.google.com
```


## Statistical Protocol Identification 
Statistical protocol identification is a technique based on divergence measurement of the traffic being analyzed and a statistical traffic model. A statistical method is used to identify different services within Skype such as Skype voice, Skype video, and Skype chat. This method uses a combination of jitter, delay, length of packets, spacing of packets, etc.

## Patterns in encrypted communication 


## DNS Linking
Clients often access DNS information prior to establishing the connection to the server. By analysis of DNS information it is possible to identify 
services in the corresponding communication. For example, the client first ask DNS to provide IP address of ```www.someservice.com```. Then the 
connection to the IP address retrieved by DNS is established. By simple reasoning this information reveals the domain name of the service of the connection.

# References

* https://www.qosmos.com/products/protocol-expertise/
* https://www.owasp.org/index.php/Transport_Layer_Protection_Cheat_Sheet#Tools