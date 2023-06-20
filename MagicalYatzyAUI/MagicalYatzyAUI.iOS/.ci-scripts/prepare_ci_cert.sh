#!/bin/bash

# Create a new keychain
security create-keychain -p "$KEYCHAIN_PASSWORD" build.keychain
# Set it as the default keychain
security default-keychain -s build.keychain
# Modify keychain settings to keep it unlocked longer
security set-keychain-settings -lut 21600 build.keychain
# Unlock the keychain so it can be used without an authorisation prompt
security unlock-keychain -p "$KEYCHAIN_PASSWORD" build.keychain

# Decode certificate to file
echo "$APPLE_DISTRIBUTION_CERT" | base64 --decode > certificate.p12
# Decode provisioning profile to file
echo "$MY_PP_ADHOC" | base64 --decode > pp.mobileprovision
# Import into keychain
security import certificate.p12 -k build.keychain -P "$CERT_PASSWORD" -A 

# Allow codesign to access keychain
security set-key-partition-list -S apple-tool:,apple:,codesign:,pkgbuild:,productsign:, -s -k "$KEYCHAIN_PASSWORD" build.keychain
security list-keychain -d user -s build.keychain

          # apply provisioning profile
mkdir -p ~/Library/MobileDevice/Provisioning\ Profiles
cp mobileprovision ~/Library/MobileDevice/Provisioning\ Profiles
