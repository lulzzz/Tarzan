meta:
  id: tls_packet
  endian: be
  license: MIT
seq:
  - id: content_type
    type: u1
    enum: tls_content_type
  
  - id: version
    type: tls_version
    
  - id: length
    type: u2

  - id: fragment
    size: length
    type:
      switch-on: content_type
      cases:
        'tls_content_type::change_cipher_spec': tls_change_cipher_spec
        'tls_content_type::alert': tls_encrypted_message
        'tls_content_type::handshake': tls_handshake
        'tls_content_type::application_data': tls_application_data
        _ : tls_encrypted_message
types:
  tls_version:
    seq:
      - id: major
        type: u1

      - id: minor
        type: u1

  tls_change_cipher_spec:
    seq:
      - id: change_message
        size-eos: true

  tls_encrypted_message:
    seq:
      - id: encrypted_message
        size-eos: true

  tls_application_data:
    seq:
      - id: body
        size-eos: true
        
        
  tls_handshake:
    seq:
      - id: msg_type
        type: u1
        enum: tls_handshake_type
      - id: length 
        type: tls_length
        if: msg_type.to_i < 32
      - id: body
        size: length.value
        if: msg_type.to_i < 32
        type:
          switch-on: msg_type
          cases:
            'tls_handshake_type::hello_request' : tls_empty
            'tls_handshake_type::client_hello': tls_client_hello
            'tls_handshake_type::server_hello': tls_server_hello
            'tls_handshake_type::certificate': tls_certificate
            'tls_handshake_type::server_key_exchange': tls_server_key_exchange
            'tls_handshake_type::certificate_request': tls_certificate_request
            'tls_handshake_type::server_hello_done': tls_server_hello_done
            'tls_handshake_type::certificate_verify': tls_certificate_verify
            'tls_handshake_type::client_key_exchange': tls_client_key_exchange
            'tls_handshake_type::finished': tls_finished
      - id: encrypted_msg
        size-eos: true


  tls_length:
    seq:
      - id: hlen
        type: u1
      - id: llen
        type: u2
    instances:
      value: 
        value: (llen + (hlen << 16))
    
  tls_empty:
    seq:
      - id: empty
        size: 0

  tls_server_hello_done:
    seq:
      - id: empty
        size: 0


  tls_certificate_verify:
    seq:
      - id: empty
        size: 0        
  
  tls_certificate_request:
    seq:
      - id: empty
        size: 0   
  
  tls_server_key_exchange:
    seq:
      - id: empty
        size: 0      
        
  tls_finished:
    seq:
      - id: verify_data
        size-eos: true         
        
  tls_certificate:
    seq:
      - id: cert_length
        type: tls_length
      
      - id: certificates
        size: cert_length.value
        type: certificate
        repeat: eos
        
  certificate:
    seq:
      - id: cert_length
        type: tls_length
        
      - id: body
        size: cert_length.value
      
  tls_server_hello: 
     seq:
      - id: version
        type: tls_version
    
      - id: random
        type: random
    
      - id: session_id
        type: session_id
    
      - id: cipher_suite
        type: cipher_suite
    
      - id: compression_methods
        type: compression_methods
    
      - id: extensions
        type: extensions
        if: _io.eof == false
  
  tls_client_hello:
    seq:
      - id: version
        type: tls_version
    
      - id: random
        type: random
    
      - id: session_id
        type: session_id
    
      - id: cipher_suites
        type: cipher_suites
    
      - id: compression_methods
        type: compression_methods
    
      - id: extensions
        type: extensions
        if: _io.eof == false

  tls_client_key_exchange: 
    seq:
      - id: exchange_keys
        size-eos: true
        
  tls_pre_master_secret:
    seq:
      - id: secret_length
        type: u2
      
      - id: secret
        size: secret_length
      
  random:
    seq:
      - id: random_time
        size: 4

      - id: random_bytes
        size: 28

  session_id:
    seq:
      - id: len
        type: u1

      - id: sid
        size: len

  cipher_suites:
    seq:
      - id: len
        type: u2
      - id: cipher_suite_list
        type: u2
        repeat: expr
        repeat-expr: len/2

  cipher_suite:
    seq:
      - id: cipher_id
        type: u2
        
  compression_methods:
    seq:
      - id: len
        type: u1

      - id: bytes
        size: len

  extensions:
    seq:
      - id: len
        type: u2

      - id: extension_list
        type: extension
        repeat: eos

  extension:
    seq:
      - id: type
        type: u2

      - id: len
        type: u2

      - id: body
        size: len
        type:
          switch-on: type
          cases:
            0: sni
            16: alpn

  sni:
    seq:
      - id: list_length
        type: u2

      - id: server_names
        type: server_name
        repeat: eos

  server_name:
    seq:
      - id: name_type
        type: u1

      - id: length
        type: u2

      - id: host_name
        size: length

  alpn:
    seq:
      - id: ext_len
        type: u2

      - id: alpn_protocols
        type: protocol
        repeat: eos

  protocol:
    seq:
      - id: strlen
        type: u1

      - id: name
        size: strlen
        
enums:
  tls_content_type:
    20: change_cipher_spec
    21: alert
    22: handshake
    23: application_data
    
  tls_handshake_type:
    0: hello_request
    1: client_hello
    2: server_hello
    4: new_session_ticket
    11: certificate
    12: server_key_exchange
    13: certificate_request
    14: server_hello_done
    15: certificate_verify
    16: client_key_exchange
    20: finished