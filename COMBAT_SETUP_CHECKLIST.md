# ?? CHECKLIST: T?i sao không gây damage?

## ?? Ki?m tra trong Unity Editor

### 1?? **PLAYER SETUP**
- [ ] Player có tag **"Player"**
- [ ] Player có component: **PlayerMovement.cs**
- [ ] Player có component: **Attack.cs**
- [ ] Player có **Rigidbody2D** (Body Type: Dynamic ho?c Kinematic)
- [ ] Player có **Collider2D** (Circle/Box/Capsule)

#### ? Aim Object (con c?a Player)
- [ ] Aim là child c?a Player
- [ ] Aim có **Transform** (?? xoay theo h??ng di chuy?n)

#### ? Melee Object (con c?a Aim)
- [ ] Melee là child c?a Aim
- [ ] Melee có **Weapon.cs** script
- [ ] Melee có **Collider2D** v?i:
  - ? **Is Trigger** = TRUE
  - ? **Enabled** = TRUE
  - ? Size ?? l?n ?? ch?m Enemy
- [ ] Melee ban ??u **SetActive(false)** trong scene

---

### 2?? **ENEMY SETUP**
- [ ] Enemy có component: **Enemy.cs**
- [ ] Enemy có **Rigidbody2D**:
  - ? Body Type: **Dynamic**
  - ? Gravity Scale: **0** (cho top-down 2D)
  - ? Freeze Rotation Z: **TRUE** (không b? xoay)
- [ ] Enemy có **Collider2D**:
  - ? **Is Trigger** = TRUE (ho?c FALSE tùy thi?t k?)
  - ? **Enabled** = TRUE
- [ ] Enemy có **maxHealth** ?ã set (m?c ??nh 10)

---

### 3?? **PHYSICS SETTINGS**

#### Layer Collision Matrix (Edit > Project Settings > Physics 2D)
Ki?m tra các layer có th? va ch?m v?i nhau:

| Layer | Collides With |
|-------|---------------|
| **Player/Weapon Layer** | ? Enemy Layer |
| **Enemy Layer** | ? Player/Weapon Layer |

#### Contact Capture Layers (n?u dùng)
- [ ] Weapon layer n?m trong Contact Capture

---

### 4?? **SCRIPT REFERENCES (trong Inspector)**

#### Attack.cs (trên Player)
- [ ] **Melee** reference ?ã ???c gán ?úng object

#### Weapon.cs (trên Melee)
- [ ] **Damage** = 2 (ho?c giá tr? mong mu?n)

---

## ?? DEBUG TRONG UNITY

### B?t Debug Logs
1. Ch?y game trong Unity Editor
2. M? **Console** window (Ctrl+Shift+C)
3. Th?c hi?n hành ??ng:
   - Di chuy?n ? ki?m tra log setup
   - Nh?n F/Click ? xem log "Attack started!"
   - Ch?m Enemy ? xem log va ch?m

### K?t qu? mong ??i trong Console:

```
=== Player Combat Setup ===
Tag: Player
? Attack script found
? Weapon script found (damage: 2)
...

Attack started! Melee active: True

Weapon hit: Enemy(Clone) (Tag: Untagged, Layer: Default)
Dealing 2 damage to Enemy(Clone)
Enemy(Clone) took 2 damage. Health: 8/10
```

---

## ? CÁC V?N ?? TH??NG G?P

### ? Không có log "Weapon hit"
**Nguyên nhân:**
- Collider không ch?m nhau (quá nh?, sai v? trí)
- Layer Collision Matrix ch?n va ch?m
- Melee object không active ?úng lúc

**Gi?i pháp:**
1. T?ng size c?a Melee Collider2D
2. Ki?m tra Layer Collision Matrix
3. Gi? F/Click ?? Melee hi?n lâu h?n (t?ng `atkDuration`)

---

### ? Có log "Weapon hit" nh?ng "No Enemy component found"
**Nguyên nhân:**
- Weapon ch?m vào Collider khác (terrain, obstacle)
- Enemy.cs script ch?a g?n vào Enemy object

**Gi?i pháp:**
1. G?n **Enemy.cs** vào Enemy prefab/object
2. Thêm tag "Enemy" và filter trong Weapon.cs:
```csharp
if (!collision.CompareTag("Enemy")) return;
```

---

### ? Có log "took damage" nh?ng Enemy không ch?t
**Nguyên nhân:**
- `maxHealth` quá cao
- Damage quá th?p
- Health reset m?i frame

**Gi?i pháp:**
1. Gi?m `maxHealth` xu?ng 2-3 ?? test
2. T?ng `damage` trong Weapon.cs lên 10+
3. Ki?m tra Enemy.cs không reset health trong Update()

---

### ? Enemy bay lung tung khi b? ?ánh
**Nguyên nhân:**
- Rigidbody2D có Body Type = Dynamic
- Không freeze rotation

**Gi?i pháp:**
```csharp
// Trong Enemy Awake()
rb.freezeRotation = true;
rb.constraints = RigidbodyConstraints2D.FreezeRotation;
```

---

## ?? QUICK TEST

### Test nhanh trong Inspector:
1. Ch?y game (Play Mode)
2. Ch?n Enemy trong Hierarchy
3. Xem component Enemy ? **Health** realtime
4. ?ánh th? ? Health có gi?m không?

### Test v?i Inspector Tool:
- Thêm **CombatDebugger.cs** vào Player/Enemy
- Xem Gizmos màu ?? hi?n collider bounds
- Ki?m tra overlap gi?a Melee và Enemy

---

## ?? N?U V?N KHÔNG WORK

### G?i thông tin sau:
1. Screenshot Console logs khi t?n công
2. Screenshot Inspector c?a:
   - Player > Attack component
   - Aim > Melee object (v?i Weapon + Collider)
   - Enemy object (v?i Enemy.cs + Rigidbody2D + Collider)
3. Screenshot Physics 2D Layer Collision Matrix

---

## ? WORKING CONFIGURATION M?U

```
Hierarchy:
- Player (Tag: Player, Layer: Default)
  ?? [Sprite/Visual]
  ?? Aim (Transform, rotate theo input)
  ?   ?? Melee (INACTIVE ban ??u)
  ?       ?? Weapon.cs (damage = 2)
  ?       ?? BoxCollider2D (IsTrigger = true, Size = 1x1)
  ?? Components:
      - Rigidbody2D (Dynamic, Gravity=0)
      - CircleCollider2D
      - PlayerMovement.cs
      - Attack.cs (reference: Melee)

- Enemy (Tag: Untagged, Layer: Default)
  ?? [Sprite/Visual]
  ?? Components:
      - Enemy.cs (maxHealth = 10)
      - Rigidbody2D (Dynamic, Gravity=0, Freeze Rotation)
      - CircleCollider2D (IsTrigger = true)
```

**Physics 2D Settings:**
- Default layer collides with Default layer: ? ENABLED
