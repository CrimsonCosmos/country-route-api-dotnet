#!/usr/bin/env bash
# Builds the React UI into wwwroot, publishes the .NET app, and deploys it to
# Azure App Service (Linux, Free F1 tier by default). Requires `az login`.
#
# Usage: ./deploy.sh [app-name]   (app name must be globally unique)
set -euo pipefail

APP="${1:-country-route-api-$RANDOM}"
RG="${RG:-country-route-rg}"
LOCATION="${LOCATION:-northcentralus}"
PLAN="${PLAN:-country-route-plan}"
SKU="${SKU:-F1}"
RUNTIME="${RUNTIME:-DOTNETCORE:10.0}"

cd "$(dirname "$0")"

echo "==> Building UI"
(cd web && npm ci && npm run build)

echo "==> Publishing API"
rm -rf publish publish.zip
dotnet publish src/CountryRoute.Api -c Release -o publish
(cd publish && zip -qr ../publish.zip .)

echo "==> Provisioning Azure resources ($RG / $PLAN / $APP)"
az group create -n "$RG" -l "$LOCATION" -o none
az appservice plan create -g "$RG" -n "$PLAN" --is-linux --sku "$SKU" -o none
az webapp create -g "$RG" -p "$PLAN" -n "$APP" --runtime "$RUNTIME" -o none
az webapp update -g "$RG" -n "$APP" --https-only true -o none

echo "==> Deploying"
az webapp deploy -g "$RG" -n "$APP" --src-path publish.zip --type zip

echo "==> Live at https://$APP.azurewebsites.net"
