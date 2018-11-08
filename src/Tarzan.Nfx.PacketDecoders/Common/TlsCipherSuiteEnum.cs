﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Packets.Common
{
    public enum TlsCipherSuite : uint
    {
        TLS_NULL_WITH_NULL_NULL = 0x0000,
        TLS_RSA_WITH_NULL_MD5 = 0x0001, // RFC5246
        TLS_RSA_WITH_NULL_SHA = 0x0002, // RFC5246
        TLS_RSA_EXPORT_WITH_RC4_40_MD5 = 0x0003, // RFC4346
        TLS_RSA_WITH_RC4_128_MD5 = 0x0004, // RFC5246
        TLS_RSA_WITH_RC4_128_SHA = 0x0005, // RFC5246
        TLS_RSA_EXPORT_WITH_RC2_CBC_40_MD5 = 0x0006, // RFC4346
        TLS_RSA_WITH_IDEA_CBC_SHA = 0x0007, // RFC5469
        TLS_RSA_EXPORT_WITH_DES40_CBC_SHA = 0x0008, // RFC4346
        TLS_RSA_WITH_DES_CBC_SHA = 0x0009, // RFC5469
        TLS_RSA_WITH_3DES_EDE_CBC_SHA = 0x000A, // RFC5246
        TLS_DH_DSS_EXPORT_WITH_DES40_CBC_SHA = 0x000B, // RFC4346
        TLS_DH_DSS_WITH_DES_CBC_SHA = 0x000C, // RFC5469
        TLS_DH_DSS_WITH_3DES_EDE_CBC_SHA = 0x000D, // RFC5246
        TLS_DH_RSA_EXPORT_WITH_DES40_CBC_SHA = 0x000E, // RFC4346
        TLS_DH_RSA_WITH_DES_CBC_SHA = 0x000F, // RFC5469
        TLS_DH_RSA_WITH_3DES_EDE_CBC_SHA = 0x0010, // RFC5246
        TLS_DHE_DSS_EXPORT_WITH_DES40_CBC_SHA = 0x0011, // RFC4346
        TLS_DHE_DSS_WITH_DES_CBC_SHA = 0x0012, // RFC5469
        TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA = 0x0013, // RFC5246
        TLS_DHE_RSA_EXPORT_WITH_DES40_CBC_SHA = 0x0014, // RFC4346
        TLS_DHE_RSA_WITH_DES_CBC_SHA = 0x0015, // RFC5469
        TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA = 0x0016, // RFC5246
        TLS_DH_anon_EXPORT_WITH_RC4_40_MD5 = 0x0017, // RFC4346
        TLS_DH_anon_WITH_RC4_128_MD5 = 0x0018, // RFC5246
        TLS_DH_anon_EXPORT_WITH_DES40_CBC_SHA = 0x0019, // RFC4346
        TLS_DH_anon_WITH_DES_CBC_SHA = 0x001A, // RFC5469
        TLS_DH_anon_WITH_3DES_EDE_CBC_SHA = 0x001B, // RFC5246
        TLS_KRB5_WITH_DES_CBC_SHA = 0x001E, // RFC2712
        TLS_KRB5_WITH_3DES_EDE_CBC_SHA = 0x001F, // RFC2712
        TLS_KRB5_WITH_RC4_128_SHA = 0x0020, // RFC2712
        TLS_KRB5_WITH_IDEA_CBC_SHA = 0x0021, // RFC2712
        TLS_KRB5_WITH_DES_CBC_MD5 = 0x0022, // RFC2712
        TLS_KRB5_WITH_3DES_EDE_CBC_MD5 = 0x0023, // RFC2712
        TLS_KRB5_WITH_RC4_128_MD5 = 0x0024, // RFC2712
        TLS_KRB5_WITH_IDEA_CBC_MD5 = 0x0025, // RFC2712
        TLS_KRB5_EXPORT_WITH_DES_CBC_40_SHA = 0x0026, // RFC2712
        TLS_KRB5_EXPORT_WITH_RC2_CBC_40_SHA = 0x0027, // RFC2712
        TLS_KRB5_EXPORT_WITH_RC4_40_SHA = 0x0028, // RFC2712
        TLS_KRB5_EXPORT_WITH_DES_CBC_40_MD5 = 0x0029, // RFC2712
        TLS_KRB5_EXPORT_WITH_RC2_CBC_40_MD5 = 0x002A, // RFC2712
        TLS_KRB5_EXPORT_WITH_RC4_40_MD5 = 0x002B, // RFC2712
        TLS_PSK_WITH_NULL_SHA = 0x002C, // RFC4785
        TLS_DHE_PSK_WITH_NULL_SHA = 0x002D, // RFC4785
        TLS_RSA_PSK_WITH_NULL_SHA = 0x002E, // RFC4785
        TLS_RSA_WITH_AES_128_CBC_SHA = 0x002F, // RFC5246
        TLS_DH_DSS_WITH_AES_128_CBC_SHA = 0x0030, // RFC5246
        TLS_DH_RSA_WITH_AES_128_CBC_SHA = 0x0031, // RFC5246
        TLS_DHE_DSS_WITH_AES_128_CBC_SHA = 0x0032, // RFC5246
        TLS_DHE_RSA_WITH_AES_128_CBC_SHA = 0x0033, // RFC5246
        TLS_DH_anon_WITH_AES_128_CBC_SHA = 0x0034, // RFC5246
        TLS_RSA_WITH_AES_256_CBC_SHA = 0x0035, // RFC5246
        TLS_DH_DSS_WITH_AES_256_CBC_SHA = 0x0036, // RFC5246
        TLS_DH_RSA_WITH_AES_256_CBC_SHA = 0x0037, // RFC5246
        TLS_DHE_DSS_WITH_AES_256_CBC_SHA = 0x0038, // RFC5246
        TLS_DHE_RSA_WITH_AES_256_CBC_SHA = 0x0039, // RFC5246
        TLS_DH_anon_WITH_AES_256_CBC_SHA = 0x003A, // RFC5246
        TLS_RSA_WITH_NULL_SHA256 = 0x003B, // RFC5246
        TLS_RSA_WITH_AES_128_CBC_SHA256 = 0x003C, // RFC5246
        TLS_RSA_WITH_AES_256_CBC_SHA256 = 0x003D, // RFC5246
        TLS_DH_DSS_WITH_AES_128_CBC_SHA256 = 0x003E, // RFC5246
        TLS_DH_RSA_WITH_AES_128_CBC_SHA256 = 0x003F, // RFC5246
        TLS_DHE_DSS_WITH_AES_128_CBC_SHA256 = 0x0040, // RFC5246
        TLS_RSA_WITH_CAMELLIA_128_CBC_SHA = 0x0041, // RFC5932
        TLS_DH_DSS_WITH_CAMELLIA_128_CBC_SHA = 0x0042, // RFC5932
        TLS_DH_RSA_WITH_CAMELLIA_128_CBC_SHA = 0x0043, // RFC5932
        TLS_DHE_DSS_WITH_CAMELLIA_128_CBC_SHA = 0x0044, // RFC5932
        TLS_DHE_RSA_WITH_CAMELLIA_128_CBC_SHA = 0x0045, // RFC5932
        TLS_DH_anon_WITH_CAMELLIA_128_CBC_SHA = 0x0046, // RFC5932
        TLS_RSA_EXPORT1024_WITH_DES_CBC_SHA = 0x0062,    // http://tools.ietf.org/html/draft-ietf-tls-56-bit-ciphersuites-01
        TLS_DHE_DSS_EXPORT1024_WITH_DES_CBC_SHA = 0x0063, // http://tools.ietf.org/html/draft-ietf-tls-56-bit-ciphersuites-01
        TLS_RSA_EXPORT1024_WITH_RC4_56_SHA = 0x0064,     // http://tools.ietf.org/html/draft-ietf-tls-56-bit-ciphersuites-01
        TLS_DHE_DSS_EXPORT1024_WITH_RC4_56_SHA = 0x0065, // http://tools.ietf.org/html/draft-ietf-tls-56-bit-ciphersuites-01
        TLS_DHE_DSS_WITH_RC4_128_SHA = 0x0066,           // http://tools.ietf.org/html/draft-ietf-tls-56-bit-ciphersuites-01 = , // 
        TLS_DHE_RSA_WITH_AES_128_CBC_SHA256 = 0x0067, // RFC5246
        TLS_DH_DSS_WITH_AES_256_CBC_SHA256 = 0x0068, // RFC5246
        TLS_DH_RSA_WITH_AES_256_CBC_SHA256 = 0x0069, // RFC5246
        TLS_DHE_DSS_WITH_AES_256_CBC_SHA256 = 0x006A, // RFC5246
        TLS_DHE_RSA_WITH_AES_256_CBC_SHA256 = 0x006B, // RFC5246
        TLS_DH_anon_WITH_AES_128_CBC_SHA256 = 0x006C, // RFC5246
        TLS_DH_anon_WITH_AES_256_CBC_SHA256 = 0x006D, // RFC5246
        TLS_GOSTR341094_WITH_28147_CNT_IMIT = 0x0080, // http://tools.ietf.org/html/draft-chudov-cryptopro-cptls-04
        TLS_GOSTR341001_WITH_28147_CNT_IMIT = 0x0081, // http://tools.ietf.org/html/draft-chudov-cryptopro-cptls-04
        TLS_GOSTR341094_WITH_NULL_GOSTR3411 = 0x0082,// http://tools.ietf.org/html/draft-chudov-cryptopro-cptls-04
        TLS_GOSTR341001_WITH_NULL_GOSTR3411 = 0x0083, // http://tools.ietf.org/html/draft-chudov-cryptopro-cptls-04
        TLS_RSA_WITH_CAMELLIA_256_CBC_SHA = 0x0084, // RFC5932
        TLS_DH_DSS_WITH_CAMELLIA_256_CBC_SHA = 0x0085, // RFC5932
        TLS_DH_RSA_WITH_CAMELLIA_256_CBC_SHA = 0x0086, // RFC5932
        TLS_DHE_DSS_WITH_CAMELLIA_256_CBC_SHA = 0x0087, // RFC5932
        TLS_DHE_RSA_WITH_CAMELLIA_256_CBC_SHA = 0x0088, // RFC5932
        TLS_DH_anon_WITH_CAMELLIA_256_CBC_SHA = 0x0089, // RFC5932
        TLS_PSK_WITH_RC4_128_SHA = 0x008A, // RFC4279
        TLS_PSK_WITH_3DES_EDE_CBC_SHA = 0x008B, // RFC4279
        TLS_PSK_WITH_AES_128_CBC_SHA = 0x008C, // RFC4279
        TLS_PSK_WITH_AES_256_CBC_SHA = 0x008D, // RFC4279
        TLS_DHE_PSK_WITH_RC4_128_SHA = 0x008E, // RFC4279
        TLS_DHE_PSK_WITH_3DES_EDE_CBC_SHA = 0x008F, // RFC4279
        TLS_DHE_PSK_WITH_AES_128_CBC_SHA = 0x0090, // RFC4279
        TLS_DHE_PSK_WITH_AES_256_CBC_SHA = 0x0091, // RFC4279
        TLS_RSA_PSK_WITH_RC4_128_SHA = 0x0092, // RFC4279
        TLS_RSA_PSK_WITH_3DES_EDE_CBC_SHA = 0x0093, // RFC4279
        TLS_RSA_PSK_WITH_AES_128_CBC_SHA = 0x0094, // RFC4279
        TLS_RSA_PSK_WITH_AES_256_CBC_SHA = 0x0095, // RFC4279
        TLS_RSA_WITH_SEED_CBC_SHA = 0x0096, // RFC4162
        TLS_DH_DSS_WITH_SEED_CBC_SHA = 0x0097, // RFC4162
        TLS_DH_RSA_WITH_SEED_CBC_SHA = 0x0098, // RFC4162
        TLS_DHE_DSS_WITH_SEED_CBC_SHA = 0x0099, // RFC4162
        TLS_DHE_RSA_WITH_SEED_CBC_SHA = 0x009A, // RFC4162
        TLS_DH_anon_WITH_SEED_CBC_SHA = 0x009B, // RFC4162
        TLS_RSA_WITH_AES_128_GCM_SHA256 = 0x009C, // RFC5288
        TLS_RSA_WITH_AES_256_GCM_SHA384 = 0x009D, // RFC5288
        TLS_DHE_RSA_WITH_AES_128_GCM_SHA256 = 0x009E, // RFC5288
        TLS_DHE_RSA_WITH_AES_256_GCM_SHA384 = 0x009F, // RFC5288
        TLS_DH_RSA_WITH_AES_128_GCM_SHA256 = 0x00A0, // RFC5288
        TLS_DH_RSA_WITH_AES_256_GCM_SHA384 = 0x00A1, // RFC5288
        TLS_DHE_DSS_WITH_AES_128_GCM_SHA256 = 0x00A2, // RFC5288
        TLS_DHE_DSS_WITH_AES_256_GCM_SHA384 = 0x00A3, // RFC5288
        TLS_DH_DSS_WITH_AES_128_GCM_SHA256 = 0x00A4, // RFC5288
        TLS_DH_DSS_WITH_AES_256_GCM_SHA384 = 0x00A5, // RFC5288
        TLS_DH_anon_WITH_AES_128_GCM_SHA256 = 0x00A6, // RFC5288
        TLS_DH_anon_WITH_AES_256_GCM_SHA384 = 0x00A7, // RFC5288
        TLS_PSK_WITH_AES_128_GCM_SHA256 = 0x00A8, // RFC5487
        TLS_PSK_WITH_AES_256_GCM_SHA384 = 0x00A9, // RFC5487
        TLS_DHE_PSK_WITH_AES_128_GCM_SHA256 = 0x00AA, // RFC5487
        TLS_DHE_PSK_WITH_AES_256_GCM_SHA384 = 0x00AB, // RFC5487
        TLS_RSA_PSK_WITH_AES_128_GCM_SHA256 = 0x00AC, // RFC5487
        TLS_RSA_PSK_WITH_AES_256_GCM_SHA384 = 0x00AD, // RFC5487
        TLS_PSK_WITH_AES_128_CBC_SHA256 = 0x00AE, // RFC5487
        TLS_PSK_WITH_AES_256_CBC_SHA384 = 0x00AF, // RFC5487
        TLS_PSK_WITH_NULL_SHA256 = 0x00B0, // RFC5487
        TLS_PSK_WITH_NULL_SHA384 = 0x00B1, // RFC5487
        TLS_DHE_PSK_WITH_AES_128_CBC_SHA256 = 0x00B2, // RFC5487
        TLS_DHE_PSK_WITH_AES_256_CBC_SHA384 = 0x00B3, // RFC5487
        TLS_DHE_PSK_WITH_NULL_SHA256 = 0x00B4, // RFC5487
        TLS_DHE_PSK_WITH_NULL_SHA384 = 0x00B5, // RFC5487
        TLS_RSA_PSK_WITH_AES_128_CBC_SHA256 = 0x00B6, // RFC5487
        TLS_RSA_PSK_WITH_AES_256_CBC_SHA384 = 0x00B7, // RFC5487
        TLS_RSA_PSK_WITH_NULL_SHA256 = 0x00B8, // RFC5487
        TLS_RSA_PSK_WITH_NULL_SHA384 = 0x00B9, // RFC5487
        TLS_RSA_WITH_CAMELLIA_128_CBC_SHA256 = 0x00BA, // RFC5932
        TLS_DH_DSS_WITH_CAMELLIA_128_CBC_SHA256 = 0x00BB, // RFC5932
        TLS_DH_RSA_WITH_CAMELLIA_128_CBC_SHA256 = 0x00BC, // RFC5932
        TLS_DHE_DSS_WITH_CAMELLIA_128_CBC_SHA256 = 0x00BD, // RFC5932
        TLS_DHE_RSA_WITH_CAMELLIA_128_CBC_SHA256 = 0x00BE, // RFC5932
        TLS_DH_anon_WITH_CAMELLIA_128_CBC_SHA256 = 0x00BF, // RFC5932
        TLS_EMPTY_RENEGOTIATION_INFO_SCSV = 0x00FF, // RFC5746
        TLS_RSA_WITH_CAMELLIA_256_CBC_SHA256 = 0x00C0, // RFC5932
        TLS_DH_DSS_WITH_CAMELLIA_256_CBC_SHA256 = 0x00C1, // RFC5932
        TLS_DH_RSA_WITH_CAMELLIA_256_CBC_SHA256 = 0x00C2, // RFC5932
        TLS_DHE_DSS_WITH_CAMELLIA_256_CBC_SHA256 = 0x00C3, // RFC5932
        TLS_DHE_RSA_WITH_CAMELLIA_256_CBC_SHA256 = 0x00C4, // RFC5932
        TLS_DH_anon_WITH_CAMELLIA_256_CBC_SHA256 = 0x00C5, // RFC5932
        TLS_ECDH_ECDSA_WITH_NULL_SHA = 0xC001, // RFC4492
        TLS_ECDH_ECDSA_WITH_RC4_128_SHA = 0xC002, // RFC4492
        TLS_ECDH_ECDSA_WITH_3DES_EDE_CBC_SHA = 0xC003, // RFC4492
        TLS_ECDH_ECDSA_WITH_AES_128_CBC_SHA = 0xC004, // RFC4492
        TLS_ECDH_ECDSA_WITH_AES_256_CBC_SHA = 0xC005, // RFC4492
        TLS_ECDHE_ECDSA_WITH_NULL_SHA = 0xC006, // RFC4492
        TLS_ECDHE_ECDSA_WITH_RC4_128_SHA = 0xC007, // RFC4492
        TLS_ECDHE_ECDSA_WITH_3DES_EDE_CBC_SHA = 0xC008, // RFC4492
        TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA = 0xC009, // RFC4492
        TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA = 0xC00A, // RFC4492
        TLS_ECDH_RSA_WITH_NULL_SHA = 0xC00B, // RFC4492
        TLS_ECDH_RSA_WITH_RC4_128_SHA = 0xC00C, // RFC4492
        TLS_ECDH_RSA_WITH_3DES_EDE_CBC_SHA = 0xC00D, // RFC4492
        TLS_ECDH_RSA_WITH_AES_128_CBC_SHA = 0xC00E, // RFC4492
        TLS_ECDH_RSA_WITH_AES_256_CBC_SHA = 0xC00F, // RFC4492
        TLS_ECDHE_RSA_WITH_NULL_SHA = 0xC010, // RFC4492
        TLS_ECDHE_RSA_WITH_RC4_128_SHA = 0xC011, // RFC4492
        TLS_ECDHE_RSA_WITH_3DES_EDE_CBC_SHA = 0xC012, // RFC4492
        TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA = 0xC013, // RFC4492
        TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA = 0xC014, // RFC4492
        TLS_ECDH_anon_WITH_NULL_SHA = 0xC015, // RFC4492
        TLS_ECDH_anon_WITH_RC4_128_SHA = 0xC016, // RFC4492
        TLS_ECDH_anon_WITH_3DES_EDE_CBC_SHA = 0xC017, // RFC4492
        TLS_ECDH_anon_WITH_AES_128_CBC_SHA = 0xC018, // RFC4492
        TLS_ECDH_anon_WITH_AES_256_CBC_SHA = 0xC019, // RFC4492
        TLS_SRP_SHA_WITH_3DES_EDE_CBC_SHA = 0xC01A, // RFC5054
        TLS_SRP_SHA_RSA_WITH_3DES_EDE_CBC_SHA = 0xC01B, // RFC5054
        TLS_SRP_SHA_DSS_WITH_3DES_EDE_CBC_SHA = 0xC01C, // RFC5054
        TLS_SRP_SHA_WITH_AES_128_CBC_SHA = 0xC01D, // RFC5054
        TLS_SRP_SHA_RSA_WITH_AES_128_CBC_SHA = 0xC01E, // RFC5054
        TLS_SRP_SHA_DSS_WITH_AES_128_CBC_SHA = 0xC01F, // RFC5054
        TLS_SRP_SHA_WITH_AES_256_CBC_SHA = 0xC020, // RFC5054
        TLS_SRP_SHA_RSA_WITH_AES_256_CBC_SHA = 0xC021, // RFC5054
        TLS_SRP_SHA_DSS_WITH_AES_256_CBC_SHA = 0xC022, // RFC5054
        TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256 = 0xC023, // RFC5289
        TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA384 = 0xC024, // RFC5289
        TLS_ECDH_ECDSA_WITH_AES_128_CBC_SHA256 = 0xC025, // RFC5289
        TLS_ECDH_ECDSA_WITH_AES_256_CBC_SHA384 = 0xC026, // RFC5289
        TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256 = 0xC027, // RFC5289
        TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384 = 0xC028, // RFC5289
        TLS_ECDH_RSA_WITH_AES_128_CBC_SHA256 = 0xC029, // RFC5289
        TLS_ECDH_RSA_WITH_AES_256_CBC_SHA384 = 0xC02A, // RFC5289
        TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256 = 0xC02B, // RFC5289
        TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384 = 0xC02C, // RFC5289
        TLS_ECDH_ECDSA_WITH_AES_128_GCM_SHA256 = 0xC02D, // RFC5289
        TLS_ECDH_ECDSA_WITH_AES_256_GCM_SHA384 = 0xC02E, // RFC5289
        TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256 = 0xC02F, // RFC5289
        TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384 = 0xC030, // RFC5289
        TLS_ECDH_RSA_WITH_AES_128_GCM_SHA256 = 0xC031, // RFC5289
        TLS_ECDH_RSA_WITH_AES_256_GCM_SHA384 = 0xC032, // RFC5289
        TLS_ECDHE_PSK_WITH_RC4_128_SHA = 0xC033, // RFC5489
        TLS_ECDHE_PSK_WITH_3DES_EDE_CBC_SHA = 0xC034, // RFC5489
        TLS_ECDHE_PSK_WITH_AES_128_CBC_SHA = 0xC035, // RFC5489
        TLS_ECDHE_PSK_WITH_AES_256_CBC_SHA = 0xC036, // RFC5489
        TLS_ECDHE_PSK_WITH_AES_128_CBC_SHA256 = 0xC037, // RFC5489
        TLS_ECDHE_PSK_WITH_AES_256_CBC_SHA384 = 0xC038, // RFC5489
        TLS_ECDHE_PSK_WITH_NULL_SHA = 0xC039, // RFC5489
        TLS_ECDHE_PSK_WITH_NULL_SHA256 = 0xC03A, // RFC5489
        TLS_ECDHE_PSK_WITH_NULL_SHA384 = 0xC03B, // RFC5489
        TLS_RSA_WITH_ARIA_128_CBC_SHA256 = 0xC03C, // RFC6209
        TLS_RSA_WITH_ARIA_256_CBC_SHA384 = 0xC03D, // RFC6209
        TLS_DH_DSS_WITH_ARIA_128_CBC_SHA256 = 0xC03E, // RFC6209
        TLS_DH_DSS_WITH_ARIA_256_CBC_SHA384 = 0xC03F, // RFC6209
        TLS_DH_RSA_WITH_ARIA_128_CBC_SHA256 = 0xC040, // RFC6209
        TLS_DH_RSA_WITH_ARIA_256_CBC_SHA384 = 0xC041, // RFC6209
        TLS_DHE_DSS_WITH_ARIA_128_CBC_SHA256 = 0xC042, // RFC6209
        TLS_DHE_DSS_WITH_ARIA_256_CBC_SHA384 = 0xC043, // RFC6209
        TLS_DHE_RSA_WITH_ARIA_128_CBC_SHA256 = 0xC044, // RFC6209
        TLS_DHE_RSA_WITH_ARIA_256_CBC_SHA384 = 0xC045, // RFC6209
        TLS_DH_anon_WITH_ARIA_128_CBC_SHA256 = 0xC046, // RFC6209
        TLS_DH_anon_WITH_ARIA_256_CBC_SHA384 = 0xC047, // RFC6209
        TLS_ECDHE_ECDSA_WITH_ARIA_128_CBC_SHA256 = 0xC048, // RFC6209
        TLS_ECDHE_ECDSA_WITH_ARIA_256_CBC_SHA384 = 0xC049, // RFC6209
        TLS_ECDH_ECDSA_WITH_ARIA_128_CBC_SHA256 = 0xC04A, // RFC6209
        TLS_ECDH_ECDSA_WITH_ARIA_256_CBC_SHA384 = 0xC04B, // RFC6209
        TLS_ECDHE_RSA_WITH_ARIA_128_CBC_SHA256 = 0xC04C, // RFC6209
        TLS_ECDHE_RSA_WITH_ARIA_256_CBC_SHA384 = 0xC04D, // RFC6209
        TLS_ECDH_RSA_WITH_ARIA_128_CBC_SHA256 = 0xC04E, // RFC6209
        TLS_ECDH_RSA_WITH_ARIA_256_CBC_SHA384 = 0xC04F, // RFC6209
        TLS_RSA_WITH_ARIA_128_GCM_SHA256 = 0xC050, // RFC6209
        TLS_RSA_WITH_ARIA_256_GCM_SHA384 = 0xC051, // RFC6209
        TLS_DHE_RSA_WITH_ARIA_128_GCM_SHA256 = 0xC052, // RFC6209
        TLS_DHE_RSA_WITH_ARIA_256_GCM_SHA384 = 0xC053, // RFC6209
        TLS_DH_RSA_WITH_ARIA_128_GCM_SHA256 = 0xC054, // RFC6209
        TLS_DH_RSA_WITH_ARIA_256_GCM_SHA384 = 0xC055, // RFC6209
        TLS_DHE_DSS_WITH_ARIA_128_GCM_SHA256 = 0xC056, // RFC6209
        TLS_DHE_DSS_WITH_ARIA_256_GCM_SHA384 = 0xC057, // RFC6209
        TLS_DH_DSS_WITH_ARIA_128_GCM_SHA256 = 0xC058, // RFC6209
        TLS_DH_DSS_WITH_ARIA_256_GCM_SHA384 = 0xC059, // RFC6209
        TLS_DH_anon_WITH_ARIA_128_GCM_SHA256 = 0xC05A, // RFC6209
        TLS_DH_anon_WITH_ARIA_256_GCM_SHA384 = 0xC05B, // RFC6209
        TLS_ECDHE_ECDSA_WITH_ARIA_128_GCM_SHA256 = 0xC05C, // RFC6209
        TLS_ECDHE_ECDSA_WITH_ARIA_256_GCM_SHA384 = 0xC05D, // RFC6209
        TLS_ECDH_ECDSA_WITH_ARIA_128_GCM_SHA256 = 0xC05E, // RFC6209
        TLS_ECDH_ECDSA_WITH_ARIA_256_GCM_SHA384 = 0xC05F, // RFC6209
        TLS_ECDHE_RSA_WITH_ARIA_128_GCM_SHA256 = 0xC060, // RFC6209
        TLS_ECDHE_RSA_WITH_ARIA_256_GCM_SHA384 = 0xC061, // RFC6209
        TLS_ECDH_RSA_WITH_ARIA_128_GCM_SHA256 = 0xC062, // RFC6209
        TLS_ECDH_RSA_WITH_ARIA_256_GCM_SHA384 = 0xC063, // RFC6209
        TLS_PSK_WITH_ARIA_128_CBC_SHA256 = 0xC064, // RFC6209
        TLS_PSK_WITH_ARIA_256_CBC_SHA384 = 0xC065, // RFC6209
        TLS_DHE_PSK_WITH_ARIA_128_CBC_SHA256 = 0xC066, // RFC6209
        TLS_DHE_PSK_WITH_ARIA_256_CBC_SHA384 = 0xC067, // RFC6209
        TLS_RSA_PSK_WITH_ARIA_128_CBC_SHA256 = 0xC068, // RFC6209
        TLS_RSA_PSK_WITH_ARIA_256_CBC_SHA384 = 0xC069, // RFC6209
        TLS_PSK_WITH_ARIA_128_GCM_SHA256 = 0xC06A, // RFC6209
        TLS_PSK_WITH_ARIA_256_GCM_SHA384 = 0xC06B, // RFC6209
        TLS_DHE_PSK_WITH_ARIA_128_GCM_SHA256 = 0xC06C, // RFC6209
        TLS_DHE_PSK_WITH_ARIA_256_GCM_SHA384 = 0xC06D, // RFC6209
        TLS_RSA_PSK_WITH_ARIA_128_GCM_SHA256 = 0xC06E, // RFC6209
        TLS_RSA_PSK_WITH_ARIA_256_GCM_SHA384 = 0xC06F, // RFC6209
        TLS_ECDHE_PSK_WITH_ARIA_128_CBC_SHA256 = 0xC070, // RFC6209
        TLS_ECDHE_PSK_WITH_ARIA_256_CBC_SHA384 = 0xC071, // RFC6209
        TLS_ECDHE_ECDSA_WITH_CAMELLIA_128_CBC_SHA256 = 0xC072, // RFC6367
        TLS_ECDHE_ECDSA_WITH_CAMELLIA_256_CBC_SHA384 = 0xC073, // RFC6367
        TLS_ECDH_ECDSA_WITH_CAMELLIA_128_CBC_SHA256 = 0xC074, // RFC6367
        TLS_ECDH_ECDSA_WITH_CAMELLIA_256_CBC_SHA384 = 0xC075, // RFC6367
        TLS_ECDHE_RSA_WITH_CAMELLIA_128_CBC_SHA256 = 0xC076, // RFC6367
        TLS_ECDHE_RSA_WITH_CAMELLIA_256_CBC_SHA384 = 0xC077, // RFC6367
        TLS_ECDH_RSA_WITH_CAMELLIA_128_CBC_SHA256 = 0xC078, // RFC6367
        TLS_ECDH_RSA_WITH_CAMELLIA_256_CBC_SHA384 = 0xC079, // RFC6367
        TLS_RSA_WITH_CAMELLIA_128_GCM_SHA256 = 0xC07A, // RFC6367
        TLS_RSA_WITH_CAMELLIA_256_GCM_SHA384 = 0xC07B, // RFC6367
        TLS_DHE_RSA_WITH_CAMELLIA_128_GCM_SHA256 = 0xC07C, // RFC6367
        TLS_DHE_RSA_WITH_CAMELLIA_256_GCM_SHA384 = 0xC07D, // RFC6367
        TLS_DH_RSA_WITH_CAMELLIA_128_GCM_SHA256 = 0xC07E, // RFC6367
        TLS_DH_RSA_WITH_CAMELLIA_256_GCM_SHA384 = 0xC07F, // RFC6367
        TLS_DHE_DSS_WITH_CAMELLIA_128_GCM_SHA256 = 0xC080, // RFC6367
        TLS_DHE_DSS_WITH_CAMELLIA_256_GCM_SHA384 = 0xC081, // RFC6367
        TLS_DH_DSS_WITH_CAMELLIA_128_GCM_SHA256 = 0xC082, // RFC6367
        TLS_DH_DSS_WITH_CAMELLIA_256_GCM_SHA384 = 0xC083, // RFC6367
        TLS_DH_anon_WITH_CAMELLIA_128_GCM_SHA256 = 0xC084, // RFC6367
        TLS_DH_anon_WITH_CAMELLIA_256_GCM_SHA384 = 0xC085, // RFC6367
        TLS_ECDHE_ECDSA_WITH_CAMELLIA_128_GCM_SHA256 = 0xC086, // RFC6367
        TLS_ECDHE_ECDSA_WITH_CAMELLIA_256_GCM_SHA384 = 0xC087, // RFC6367
        TLS_ECDH_ECDSA_WITH_CAMELLIA_128_GCM_SHA256 = 0xC088, // RFC6367
        TLS_ECDH_ECDSA_WITH_CAMELLIA_256_GCM_SHA384 = 0xC089, // RFC6367
        TLS_ECDHE_RSA_WITH_CAMELLIA_128_GCM_SHA256 = 0xC08A, // RFC6367
        TLS_ECDHE_RSA_WITH_CAMELLIA_256_GCM_SHA384 = 0xC08B, // RFC6367
        TLS_ECDH_RSA_WITH_CAMELLIA_128_GCM_SHA256 = 0xC08C, // RFC6367
        TLS_ECDH_RSA_WITH_CAMELLIA_256_GCM_SHA384 = 0xC08D, // RFC6367
        TLS_PSK_WITH_CAMELLIA_128_GCM_SHA256 = 0xC08E, // RFC6367
        TLS_PSK_WITH_CAMELLIA_256_GCM_SHA384 = 0xC08F, // RFC6367
        TLS_DHE_PSK_WITH_CAMELLIA_128_GCM_SHA256 = 0xC090, // RFC6367
        TLS_DHE_PSK_WITH_CAMELLIA_256_GCM_SHA384 = 0xC091, // RFC6367
        TLS_RSA_PSK_WITH_CAMELLIA_128_GCM_SHA256 = 0xC092, // RFC6367
        TLS_RSA_PSK_WITH_CAMELLIA_256_GCM_SHA384 = 0xC093, // RFC6367
        TLS_PSK_WITH_CAMELLIA_128_CBC_SHA256 = 0xC094, // RFC6367
        TLS_PSK_WITH_CAMELLIA_256_CBC_SHA384 = 0xC095, // RFC6367
        TLS_DHE_PSK_WITH_CAMELLIA_128_CBC_SHA256 = 0xC096, // RFC6367
        TLS_DHE_PSK_WITH_CAMELLIA_256_CBC_SHA384 = 0xC097, // RFC6367
        TLS_RSA_PSK_WITH_CAMELLIA_128_CBC_SHA256 = 0xC098, // RFC6367
        TLS_RSA_PSK_WITH_CAMELLIA_256_CBC_SHA384 = 0xC099, // RFC6367
        TLS_ECDHE_PSK_WITH_CAMELLIA_128_CBC_SHA256 = 0xC09A, // RFC6367
        TLS_ECDHE_PSK_WITH_CAMELLIA_256_CBC_SHA384 = 0xC09B, // RFC6367
        TLS_RSA_WITH_AES_128_CCM = 0xC09C, // RFC6655
        TLS_RSA_WITH_AES_256_CCM = 0xC09D, // RFC6655
        TLS_DHE_RSA_WITH_AES_128_CCM = 0xC09E, // RFC6655
        TLS_DHE_RSA_WITH_AES_256_CCM = 0xC09F, // RFC6655
        TLS_RSA_WITH_AES_128_CCM_8 = 0xC0A0, // RFC6655
        TLS_RSA_WITH_AES_256_CCM_8 = 0xC0A1, // RFC6655
        TLS_DHE_RSA_WITH_AES_128_CCM_8 = 0xC0A2, // RFC6655
        TLS_DHE_RSA_WITH_AES_256_CCM_8 = 0xC0A3, // RFC6655
        TLS_PSK_WITH_AES_128_CCM = 0xC0A4, // RFC6655
        TLS_PSK_WITH_AES_256_CCM = 0xC0A5, // RFC6655
        TLS_DHE_PSK_WITH_AES_128_CCM = 0xC0A6, // RFC6655
        TLS_DHE_PSK_WITH_AES_256_CCM = 0xC0A7, // RFC6655
        TLS_PSK_WITH_AES_128_CCM_8 = 0xC0A8, // RFC6655
        TLS_PSK_WITH_AES_256_CCM_8 = 0xC0A9, // RFC6655
        TLS_PSK_DHE_WITH_AES_128_CCM_8 = 0xC0AA, // RFC6655
        TLS_PSK_DHE_WITH_AES_256_CCM_8 = 0xC0AB, // RFC6655
        SSL_RSA_FIPS_WITH_DES_CBC_SHA = 0xFEFE,  // http://www.mozilla.org/projects/security/pki/nss/ssl/fips-ssl-ciphersuites.html
        SSL_RSA_FIPS_WITH_3DES_EDE_CBC_SHA = 0xFEFF, // http://www.mozilla.org/projects/security/pki/nss/ssl/fips-ssl-ciphersuites.html
        SSL_RSA_FIPS_WITH_3DES_EDE_CBC_SHA_ = 0xFFE0, // http://www.mozilla.org/projects/security/pki/nss/ssl/fips-ssl-ciphersuites.html
        SSL_RSA_FIPS_WITH_DES_CBC_SHA_ = 0xFFE1,  // http://www.mozilla.org/projects/security/pki/nss/ssl/fips-ssl-ciphersuites.html
        TLS_ECDHE_ECDSA_WITH_AES_128_CCM = 0xC0AC,   // [RFC-mcgrew-tls-aes-ccm-ecc-08]
        TLS_ECDHE_ECDSA_WITH_AES_256_CCM = 0xC0AD,   // [RFC-mcgrew-tls-aes-ccm-ecc-08]
        TLS_ECDHE_ECDSA_WITH_AES_128_CCM_8 = 0xC0AE,  // [RFC-mcgrew-tls-aes-ccm-ecc-08]
        TLS_ECDHE_ECDSA_WITH_AES_256_CCM_8 = 0xC0AF, // [RFC-mcgrew-tls-aes-ccm-ecc-08]

        // SSL2 Cipher suites
        SSL2_RC4_128_WITH_MD5 = 0x010080,
        SSL2_RC4_128_EXPORT40_WITH_MD5 = 0x020080,
        SSL2_RC2_CBC_128_CBC_WITH_MD5 = 0x030080,
        SSL2_RC2_CBC_128_CBC_EXPORT_WITH_MD5 = 0x040080,
        SSL2_IDEA_128_CBC_WITH_MD5 = 0x050080,
        SSL2_DES_64_CBC_WITH_MD5 = 0x060040,
        SSL2_DES_192_EDE3_CBC_WITH_MD5 = 0x0700c0,
        SSL2_RC4_64_WITH_MD5 = 0x080080
    }
}
