#!/bin/bash

APP_NAME="bin/app/MagicalYatzy.app"
ENTITLEMENTS=".macos/Info.plist"
SIGNING_IDENTITY="Developer ID Application: Anton Makarevich (Z8GJWJ4Q4E)" # matches Keychain Access certificate name

find "$APP_NAME/Contents/MacOS"|while read fname; do
    if [[ -f $fname ]]; then
        echo "[INFO] Signing $fname"
        codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_IDENTITY" "$fname"
    fi
done

echo "[INFO] Signing app file"

codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_IDENTITY" "$APP_NAME"
