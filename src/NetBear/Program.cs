using var mutex = new Mutex(true, @"Local\NetBear", out bool createdNew);
if (!createdNew) return;
