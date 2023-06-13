#!/bin/bash

APP_NAME="bin/app/MagicalYatzy.app"
PUBLISH_OUTPUT_DIRECTORY="bin/Release/osx-x64/publish/."
INFO_PLIST=".macos/Info.plist"
ICON_FILE=".macos/icon.icns"

# build the app
dotnet publish -r osx-x64 -c Release -p:UseAppHost=true --self-contained -o $PUBLISH_OUTPUT_DIRECTORY
# compile icon
iconutil -c icns icon.iconset

# build the bundle structure
if [ -d "$APP_NAME" ]
then
    rm -rf "$APP_NAME"
fi

mkdir -p $APP_NAME

mkdir -p $APP_NAME/Contents
mkdir -p $APP_NAME/Contents/MacOS
mkdir -p $APP_NAME/Contents/Resources

cp "$INFO_PLIST" "$APP_NAME/Contents/Info.plist"
cp "$ICON_FILE" "$APP_NAME/Contents/Resources/icon.icns"
cp -a "$PUBLISH_OUTPUT_DIRECTORY" "$APP_NAME/Contents/MacOS"

chmod +x $APP_NAME
