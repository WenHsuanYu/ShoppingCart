# ShoppingCart

[Demo website](https://shoppingcartui20240228220229.azurewebsites.net/)

主要使用 ASP.NET Core MVC 框架和預設 Jquery & Bootstrap，主要用到的 packages or tools 如下
- EF Core
- Secret Manager (for local)
- Azure Services
  - Azure app service
  - Azure Key Vault
  - Azure SQL Database
  - Azure Storage (for storing uploaded images)
  - Azure Identity
  - Azure ApplicationInsights (for logging and debugging)
- some codegeneration packages
- Newtonsoft.Json
- RestSharp

- 託管在 azure app service free tier (效能較差，純粹用於測試)

功能面:
- 使用者: 分成`會員`和`管理者`
- Authentication & Role-based authorization 或者外部登入(Google，但限於測試使用者)
- 會員
  - 可以將商品加入購物車
  - 在購物車中進行商品數量增減
  - 結帳 (未做API串接)，建立訂單
  - 在設定中，查看訂單
  - 變更密碼、新增手機號碼
  - 下載個人資料 & 刪除帳號
- 管理者
   - 包含所有會員功能
   - 對於商品進行 CRUD
     - Create 有上傳功能，可以上傳圖片，並針對檔名進行隨機名稱處理，透過串接 VirusTotal API 檢測可疑檔案並對於圖片進行檔案類型限制和檔案大小的限制，
       視檔案類型進行 imgur API 串接或者存入 Azure storage 並下載到特定處。
   - 對於所有訂單，進行查看、編輯、個別訂單刪除
