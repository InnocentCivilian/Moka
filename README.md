# Moka
Moka is a (Potentially Federated) Trust-based Messaging concept. Its goal is to provide an End-to-End encryption among humans. 
# Current stage
We have a SDK , tests and proof of concept .[WE HAVE NOT developed any new encryption](https://keasigmadelta.com/blog/why-you-shouldnt-write-your-own-encryption-algorithm/) algorithms and completely used community trusted algorithms and concepts for this purpose.However our main intention is to present a methodology,Therefore everything we used are totally replaceable.
## Algorithms
- AES was used for symmetric encryption of conversations between humans. it's generated per message and single-used.
- RSA was used for asymmetric encryption and to exchange keys between humans.
## Chain of Trust (CoT)
TLS certificate verification works perfectly as of now but it's for computers NOT HUMANS :) So we took this concept and wrote a human and server based chain of trust mechanism.This trust chain works much similar to TLS, you can set an expiration date, have your own certificate authority and give others permission to issue a certificate for humans / servers.
## (Potentially) Federated
Along with CoT we’d like users to choose their own server (like the OG email) and have their own rules for themselves. Therefore see Moka ultimately as a Federated E2E messenger.

# Caution
I’m not a cryptography expert and obviously this project is not production ready! Please use at your own risk!
# How can I contribute?
Feel free to add anything. This project is my long-term hobby and it will improve overtime.
# License
MIT
