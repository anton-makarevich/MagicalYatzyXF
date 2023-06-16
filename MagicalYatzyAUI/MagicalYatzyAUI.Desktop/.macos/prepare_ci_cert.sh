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
echo "$DEV_ID_CERT" | base64 --decode > certificate.p12
echo "$DEV_ID_INSTALLER_CERT" | base64 --decode > certificate_inst.p12
# Import into keychain
security import certificate.p12 -k build.keychain -P "$CERT_PASSWORD" -T /usr/bin/codesign 
security import certificate_inst.p12 -k build.keychain -P "$CERT_PASSWORD" -T /usr/bin/productsign

# Allow codesign to access keychain
security set-key-partition-list -S apple-tool:,apple:,codesign:,pkgbuild:,productsign:, -s -k "$KEYCHAIN_PASSWORD" build.keychain
security list-keychain -d user -s build.keychain

xcrun notarytool store-credentials "AC_PASSWORD" --apple-id "$APPLE_ID" --team-id "$TEAM_ID" --password "$NOTARY_TOOL_PASSWORD"
