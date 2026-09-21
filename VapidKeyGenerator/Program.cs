using WebPush;

var keys = VapidHelper.GenerateVapidKeys();

Console.WriteLine($"Public Key: {keys.PublicKey}");
Console.WriteLine($"Private Key: {keys.PrivateKey}");