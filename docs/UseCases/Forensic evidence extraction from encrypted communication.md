# Forensic evidence extraction from encrypted communication

## Annotation
Today most of the network communication is secured by encryption. While it is not possible to obtain a plain content of the communication, still encrypted communication can have some forensic value. The proposed project aims at researching methods to obtain forensic information from encrypted communication. The expected information will have form of metadata or derived information rather than the direct evidence. The project will focus on Internet communication encrypted using TLS/SSL.  

## Approach
Analyze and describe characteristics of encrypted communication. Initially start with HTTPS communication and create a dataset of encrypted and corresponding plain communication (use key logging mechanism of a web browser). Propose a method that based on the knowledge of both plain and secured communication can create a classifier for recognizing metadata in unknown secured traffic.  

## Goal
To invent a methods for creating classifiers for metadata extraction and derived evidence identification in encrypted traffic. Demonstrate and evaluate the proposed system for a reasonable big dataset. 