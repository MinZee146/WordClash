using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genix.MocaLib.Runtime.Services;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class ShopManager : SingletonPersistent<ShopManager>
{
    [SerializeField] private GameObject _adItem, _bundleItem, _coinItem, _restorePurchasesButton;
    [SerializeField] private RectTransform _adsList, _bundleList, _coinsList, _viewportRect;
    [SerializeField] private GridLayoutGroup _coinsLayout, _adsLayout;
    [SerializeField] private TextMeshProUGUI _rewardedAdCoinsText;

    private List<IAPItem> _adsPackages = new(), _bundlePackages = new(), _coinPackages = new();
    private List<ShopItem> _restorableList = new(), _shopItemList = new();

    public async void Initialize()
    {
        await LoadPackages();

#if UNITY_IOS
        _restorePurchasesButton.SetActive(true);
        _restorePurchasesButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            MocaLib.Instance.IAPManager.RestorePurchases();
            _restorePurchasesButton.SetActive(false);
        });
#else
        _restorePurchasesButton.SetActive(false);
#endif
    }

    private void RegisterRestore()
    {
        MocaLib.Instance.IAPManager.OnRestorePurchases += (Product product) =>
        {
            if (!product.hasReceipt) return;

            var itemToRestore = _restorableList.FirstOrDefault(item => item.ProductId == product.definition.id);

            if (itemToRestore?.PackageData is Bundles bundle)
            {
                if (bundle.RemoveAllAds)
                {
                    PlayerPrefs.SetInt(GameConstants.PLAYER_PREFS_IS_ADS_ENABLED, 0);

                    MocaLib.Instance.AdManager.HideBannerAd();
                    NavigationBarController.Instance.UpdatePositionBasedOnAds(false);
                }

                return;
            }

            itemToRestore?.Restore();
        };
    }

    private async Task LoadPackages()
    {
        try
        {
            var adsHandle = Addressables.LoadAssetsAsync<IAPItem>("Ads", null);
            var bundleHandle = Addressables.LoadAssetsAsync<IAPItem>("Bundle", null);
            var coinHandle = Addressables.LoadAssetsAsync<IAPItem>("Coin", null);

            var adsPackages = await adsHandle.Task;
            var bundlePackages = await bundleHandle.Task;
            var coinPackages = await coinHandle.Task;

            if (adsPackages != null && bundlePackages != null && coinPackages != null)
            {
                _adsPackages = adsPackages.OrderBy(item => item.Price).ToList();
                _bundlePackages = bundlePackages.OrderBy(item => item.Price).ToList();
                _coinPackages = coinPackages.OrderBy(item => item.Price).ToList();

                var productList = _adsPackages.Select(item => (item.ProductId, item.ProductType)).ToList();
                productList.AddRange(_bundlePackages.Select(item => (item.ProductId, item.ProductType)));
                productList.AddRange(_coinPackages.Select(item => (item.ProductId, item.ProductType)));

                LoadShopItem();
                MocaLib.Instance.IAPManager.Initialize(productList, onInitialized: () =>
                {
#if !UNITY_EDITOR
                    ReloadShopItemData();
#endif
                });

                Utils.Log($"Successfully loaded {_adsPackages.Count + _bundlePackages.Count + _coinPackages.Count} packages.");
            }
            else
            {
                Utils.LogError("Failed to load package list or no packages found.");
            }
        }
        catch (Exception ex)
        {
            Utils.LogError($"Error loading packages: {ex.Message}");
        }
    }

    private void LoadShopItem()
    {
        _rewardedAdCoinsText.text = RemoteConfigs.Instance.GameConfigs.CoinsPerAd.ToString();

        _adsList.sizeDelta = new((_adsLayout.cellSize.x + _adsLayout.spacing.x) * _adsLayout.constraintCount,
                                    (_adsLayout.cellSize.y + _adsLayout.spacing.y) * Mathf.CeilToInt((float)_adsPackages.Count / _coinsLayout.constraintCount));
        _coinsList.sizeDelta = new((_coinsLayout.cellSize.x + _coinsLayout.spacing.x) * _coinsLayout.constraintCount,
                                    (_coinsLayout.cellSize.y + _coinsLayout.spacing.y) * Mathf.CeilToInt((float)_coinPackages.Count / _coinsLayout.constraintCount) + 100f);

        var _adsListRect = _adsList.transform.GetChild(0);
        var _coinsListRect = _coinsList.transform.GetChild(0);

        foreach (var t in _adsPackages)
        {
            var ads = Instantiate(_adItem, _adsListRect);
            var component = ads.GetComponent<ShopItem>();

            component.PackageData = t;
            component.LoadData(t);
            _restorableList.Add(component);
            _shopItemList.Add(component);
        }

        foreach (var t in _bundlePackages)
        {
            var bundle = Instantiate(_bundleItem, _bundleList.transform);
            var component = bundle.GetComponent<ShopItem>();

            component.PackageData = t;
            component.LoadData(t);
            _restorableList.Add(component);
            _shopItemList.Add(component);
        }

        foreach (var t in _coinPackages)
        {
            var coinPack = Instantiate(_coinItem, _coinsListRect);
            var component = coinPack.GetComponent<ShopItem>();

            component.PackageData = t;
            component.LoadData(t);
            _shopItemList.Add(component);
        }
    }

    private void ReloadShopItemData()
    {
        foreach (var shopItem in _shopItemList)
        {
            shopItem.IsReloadData = true;
            shopItem.LoadData(shopItem.PackageData);
        }

        RegisterRestore();
    }

    public void UpdateViewPortBasedOnAds(float offsetY)
    {
        _viewportRect.offsetMin = new Vector2(0, offsetY);
    }
}
