#!/bin/bash

# Create a new keychain
security create-keychain -p "${{ secrets.KEYCHAIN_PASSWORD}}" build.keychain
# Set it as the default keychain
security default-keychain -s build.keychain
# Unlock the keychain so it can be used without an authorisation prompt
security unlock-keychain -p "${{ secrets.KEYCHAIN_PASSWORD}}" build.keychain

# Decode certificate to file
echo "${{ secrets.DEV_ID_CERT }}" | base64 --decode > certificate.p12
# Import into keychain
security import certificate.p12 -k build.keychain -P "${{ secrets.CERT_PASSWORD }}" -T /usr/bin/codesign

# Allow codesign to access keychain
security set-key-partition-list -S apple-tool:,apple:,codesign: -s -k "${{ secrets.KEYCHAIN_PASSWORD}}" build.keychain

xcrun notarytool store-credentials "AC_PASSWORD" --apple-id "${{ secrets.APPLE_ID }}" --team-id "${{ env.TEAM_ID }}" --password "${{ secrets.NOTARY_TOOL_PASSWORD }}"
