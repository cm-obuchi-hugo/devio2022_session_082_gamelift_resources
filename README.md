## 事前準備するもの    
- AWSアカウント
- .NET 6.0
- Java SE 8 (ローカルテスト用)
- 新しく作ったUnityプロジェクト
- AWS CLI (全てGameLift権限を持つIAMユーザープロフィール・Adminでも構いません)
https://dev.classmethod.jp/articles/amazon_gamelift_walkthrough_in_csharp_02/
- Server側が必要なGameLift SDK
GameLift Managed Servers SDK：https://aws.amazon.com/gamelift/getting-started/
- Client側が必要なGameLift SDK(dlls) (nupkg -> zip)
https://www.nuget.org/packages/devio.amazon.gamelift.client.lib.collection
- Microsoft.Bcl.AsyncInterfaces.dll
https://www.nuget.org/packages/Microsoft.Bcl.AsyncInterfaces/

```
Mirror Networking:
https://assetstore.unity.com/packages/tools/network/mirror-129321

ParrelSync:
https://github.com/VeriorPies/ParrelSync
```

&nbsp;  
&nbsp;  


## メインの関数
```csharp  
GameLiftClient.DescribeGameSessionsAsync(request);

GameLiftClient.CreateGameSessionAsync(request);

GameLiftClient.CreatePlayerSessionAsync(request);
```

The Elevator Bossa Nova - Benjamin Tissot  

From Music: bensound.com

