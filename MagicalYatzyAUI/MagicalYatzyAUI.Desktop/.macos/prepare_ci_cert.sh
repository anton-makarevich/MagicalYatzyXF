#!/bin/bash

# Create a new keychain
security create-keychain -p "$KEYCHAIN_PASSWORD" build.keychain
# Set it as the default keychain
security default-keychain -s build.keychain
# Unlock the keychain so it can be used without an authorisation prompt
security unlock-keychain -p "$KEYCHAIN_PASSWORD" build.keychain

# Decode certificate to file
echo "$DEV_ID_CERT" | base64 --decode > certificate.p12
# Import into keychain
security import certificate.p12 -k build.keychain -P "$CERT_PASSWORD" -T /usr/bin/codesign

# Allow codesign to access keychain
security set-key-partition-list -S apple-tool:,apple:,codesign: -s -k "$KEYCHAIN_PASSWORD" build.keychain

xcrun notarytool store-credentials "AC_PASSWORD" --apple-id "$APPLE_ID" --team-id "$TEAM_ID" --password "$NOTARY_TOOL_PASSWORD"
