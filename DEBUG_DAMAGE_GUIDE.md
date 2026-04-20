# ?? H??NG D?N DEBUG: Không gây damage cho Enemy

## ?? B?N ?Ã CÓ GÌ

### ? Scripts ?ã ???c c?i thi?n:
1. **Enemy.cs** - ?ã thêm:
   - ? Debug logs (màu ??/vàng)
   - ? Visual feedback (flash màu ?? khi b? ?ánh)
   - ? S?a bug `rb.velocity` thay vì `angularVelocity`

2. **Weapon.cs** - ?ã thêm:
   - ? Debug logs chi ti?t (màu xanh)
   - ? Optional tag filtering
   - ? Log m?i collision (k? c? không ph?i Enemy)

3. **Attack.cs** - ?ã thêm:
   - ? Log khi attack ???c trigger

4. **CombatDebugger.cs** (NEW)
   - ? Hi?n th? setup info khi game start
   - ? V? Gizmos ?? xem collider bounds

---

## ?? CÁCH S? D?NG

### B??c 1: Ch?y game và xem Console
1. Trong Unity, ch?y game (Play button)
2. M? **Console window** (Window > General > Console ho?c Ctrl+Shift+C)
3. Di chuy?n Player ? Attack (F ho?c Click)

### B??c 2: Phân tích logs

#### ? N?u th?y logs này = WORKING:
```
Attack started! Melee active: True
Weapon hit: Enemy (Tag: Untagged, Layer: Default)
Dealing 2 damage to Enemy
Enemy took 2 damage. Health: 8/10
```
? **H? th?ng ho?t ??ng!** Enemy s? flash ?? và ch?t sau 5 ?òn (2 damage x 5 = 10 HP)

---

#### ? KHÔNG th?y "Attack started!"
**V?n ??:** Attack script không nh?n input

**Gi?i pháp:**
1. Ki?m tra `Attack.cs` có g?n trên Player
2. Th? nh?n **F** ho?c **gi? chu?t trái**
3. Ki?m tra game không b? pause

---

#### ? Th?y "Attack started!" nh?ng KHÔNG th?y "Weapon hit"
**V?n ??:** Melee collider không ch?m Enemy

**Gi?i pháp:**

##### A) Ki?m tra Melee Setup:
```
Melee GameObject:
?? Weapon.cs (damage = 2)
?? BoxCollider2D ho?c CircleCollider2D
   ?? Is Trigger: ? TRUE
   ?? Enabled: ? TRUE
   ?? Size: ?? l?n (ít nh?t 0.5x0.5)
```

##### B) Test trong Scene View:
1. Ch?y game (Play Mode)
2. Nh?n F ?? attack
3. Trong **Scene view**, ch?n Melee object
4. Xem collider (màu xanh) có overlap Enemy không?

**N?u không overlap:**
- T?ng size c?a Melee collider
- Ho?c di chuy?n Melee g?n Enemy h?n

##### C) S? d?ng CombatDebugger:
1. G?n `CombatDebugger.cs` vào Player
2. Trong Scene view, s? th?y **Gizmos màu ??** v? collider bounds
3. Ki?m tra Melee và Enemy collider có overlap không

---

#### ? Th?y "Weapon hit" nh?ng "No Enemy component found"
**V?n ??:** Weapon ch?m object khác ho?c Enemy thi?u script

**Log m?u:**
```
Weapon hit: Wall (Tag: Untagged, Layer: Default)
No Enemy component found on Wall
```

**Gi?i pháp:**

##### A) N?u hit ?úng Enemy object:
1. Ch?n Enemy trong Hierarchy
2. Ki?m tra có component **Enemy.cs** không
3. N?u không ? Add Component > Enemy

##### B) N?u hit sai object (Wall, Obstacle...):
**Option 1:** Dùng Tag filtering
```csharp
// Trong Weapon.cs Inspector
Target Tag: "Enemy"  // Ch? ?ánh object có tag này
```

**Option 2:** T?o tag "Enemy":
1. Ch?n Enemy object
2. Tag dropdown > Add Tag...
3. T?o tag m?i: "Enemy"
4. Gán tag cho Enemy object

**Option 3:** Dùng Layer:
1. T?o layer "Enemy" (Layer dropdown > Add Layer...)
2. Gán Enemy vào layer này
3. Weapon ch? collide v?i layer "Enemy" (Physics 2D settings)

---

#### ? Th?y "took damage" nh?ng Enemy KHÔNG CH?T
**V?n ??:** Health quá cao ho?c damage quá th?p

**Log m?u:**
```
Enemy took 2 damage. Health: 8/10
Enemy took 2 damage. Health: 6/10
Enemy took 2 damage. Health: 4/10
...
```

**Gi?i pháp:**

##### Quick test:
1. Ch?n Enemy prefab/object
2. Trong `Enemy.cs`:
   - Gi?m `maxHealth` xu?ng **2** (ch?t sau 1 ?òn)
3. Ho?c trong `Weapon.cs`:
   - T?ng `damage` lên **100** (one-shot)

##### Visual check:
- Enemy ph?i **flash màu ??** m?i l?n b? ?ánh
- N?u không flash ? `flashOnHit` b? t?t ho?c không có SpriteRenderer

---

## ?? QUICK FIXES

### Fix 1: T?ng Melee Collider Size
```
Melee > BoxCollider2D:
- Size X: 1.5
- Size Y: 1.5
```

### Fix 2: Gi? Melee lâu h?n (d? test)
```csharp
// Attack.cs
float atkDuration = 1.0f;  // Thay vì 0.3f
```

### Fix 3: One-shot Enemy (test nhanh)
```csharp
// Weapon.cs
public float damage = 100f;  // Thay vì 2f
```

### Fix 4: Freeze Enemy ?? test d? h?n
```csharp
// Enemy.cs > Start()
// Comment dòng này:
// target = GameObject.FindGameObjectWithTag("Player").transform;
```

---

## ?? PHYSICS 2D SETTINGS

### Ki?m tra Layer Collision Matrix:
1. Edit > Project Settings > Physics 2D
2. Scroll xu?ng **Layer Collision Matrix**
3. ??m b?o tick vào:
   - Default ? Default
   - Player ? Enemy (n?u có t?o layer riêng)

---

## ?? TEST CASES

### Test 1: Enemy ch?t sau 5 ?òn
- Weapon damage = 2
- Enemy maxHealth = 10
- K?t qu?: 5 l?n attack ? Enemy destroyed

### Test 2: Visual feedback
- Attack ? Enemy flash màu ?? 0.1s
- Console log màu ??: "took damage"

### Test 3: Multiple enemies
- T?o 3 Enemy
- Attack 1 cái ? ch? cái ?ó nh?n damage
- Các cái khác không ?nh h??ng

---

## ?? N?U V?N L?I, G?I CHO TÔI:

1. **Screenshot Console** (toàn b? logs khi attack)
2. **Screenshot Inspector:**
   - Player > Attack component
   - Aim > Melee (Weapon + Collider)
   - Enemy (Enemy.cs + Rigidbody + Collider)
3. **Screenshot Physics 2D** Layer Collision Matrix
4. **Video** ng?n (~10s) c?a lúc attack trong Scene view

---

## ? EXPECTED RESULT

Khi attack thành công:
1. ? Console: Log màu xanh "Dealing damage"
2. ? Enemy: Flash ??
3. ? Console: Log màu ?? "took damage"
4. ? Enemy: Destroy sau ?? ?òn
5. ? Console: Log màu vàng "died!"

---

## ?? SUPPORT

N?u làm theo h?t mà v?n l?i, comment v?i info:
- Unity version
- Console logs (copy text)
- Screenshots nh? trên

Chúc may m?n! ??
