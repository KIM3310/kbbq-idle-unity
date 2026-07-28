from pathlib import Path
import json


ROOT = Path(__file__).resolve().parents[1]
PRIVATE_INQUIRY_URL = (
    "https://kim3310-doeon-kim-portfolio.pages.dev/"
    "?offer=kbbq-idle-unity&inquiry=consumer-prototype-customization#private-inquiry"
)


def require(path: str, marker: str) -> None:
    text = (ROOT / path).read_text(encoding="utf-8")
    if marker not in text:
        raise SystemExit(f"{path} is missing required boundary: {marker}")


def forbid(path: str, marker: str) -> None:
    text = (ROOT / path).read_text(encoding="utf-8")
    if marker in text:
        raise SystemExit(f"{path} contains unsafe monetization path: {marker}")


require("Assets/Scripts/Data/OptionalEconomyConfig.cs", "public bool enableAds = false;")
require("Assets/Scripts/Data/OptionalEconomyConfig.cs", "public bool enableIap = false;")
require("Assets/Scripts/Core/OptionalEconomyService.cs", "IOptionalEconomyGateway")
require("Assets/Scripts/Core/OptionalEconomyService.cs", "verifiedTransactionIds")
forbid(
    "Assets/Scripts/Core/OptionalEconomyService.cs",
    "GrantCurrency(config.interstitialReward",
)
forbid(
    "Assets/Scripts/Core/OptionalEconomyService.cs",
    "GrantCurrency(config.packs[i].currencyReward",
)
require("Assets/Data/Config/OptionalEconomyConfig.asset", "enableAds: 0")
require("Assets/Data/Config/OptionalEconomyConfig.asset", "enableIap: 0")
require("README.md", "ads and IAP are disabled by default")
require("docs/index.html", "Ads and IAP are disabled by default")
require("docs/index.html", "Request customization")
forbid("docs/index.html", "View paid options")
forbid("docs/index.html", "Paid path: cosmetic packs")
forbid("docs/service-offer.json", "Paid path: cosmetic packs")
forbid("docs/service-offer.json", "and.\",")

offer = json.loads((ROOT / "docs/service-offer.json").read_text(encoding="utf-8"))
if offer["lead_capture_url"] != PRIVATE_INQUIRY_URL:
    raise SystemExit("docs/service-offer.json does not use the central private inquiry URL")
if offer["commerce"]["lane_id"] != "consumer-prototype-customization":
    raise SystemExit("docs/service-offer.json does not use consumer-prototype-customization")
if offer["commerce"]["checkout"]["provider"] is not None:
    raise SystemExit("docs/service-offer.json must not advertise a checkout provider")
if offer["commerce"]["checkout"]["status"] != "not-configured":
    raise SystemExit("docs/service-offer.json checkout status must be not-configured")
if offer["commerce"]["checkout"]["fallback_url"] != PRIVATE_INQUIRY_URL:
    raise SystemExit("docs/service-offer.json checkout fallback must use private inquiry")
advertising = offer["commerce"]["advertising"]
if not advertising["eligible"]:
    raise SystemExit("docs/service-offer.json must mark the separate central resource as advertising eligible")
if advertising["delivery_surface"] != (
    "https://kim3310-doeon-kim-portfolio.pages.dev/resources/kbbq-idle-unity/"
):
    raise SystemExit("docs/service-offer.json must route advertising to the central resource")
if advertising["status"] != "central-resource-site-review-dependent":
    raise SystemExit("docs/service-offer.json must disclose the central resource review dependency")
if "private prototype customization" not in offer["first_paid_sku"]:
    raise SystemExit("docs/service-offer.json first paid SKU must be customization-based")
if "Ads and IAP are disabled by default" not in offer["structured_data"]["description"]:
    raise SystemExit("structured data must disclose inactive ads/IAP")
if offer["structured_data"]["offers"][1]["url"] != PRIVATE_INQUIRY_URL:
    raise SystemExit("structured data paid offer must route to private inquiry")

print("monetization boundary validation ok")
