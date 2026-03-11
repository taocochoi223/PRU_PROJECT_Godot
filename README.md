# 🎮 KỊCH BẢN CHI TIẾT — GAME THẠCH SANH 2D
*Dựa trên truyện cổ tích dân gian Việt Nam | Engine: Godot 4 (C#)*

---

## 🗺️ TỔNG QUAN 4 MÀN

| Màn | Chủ đề | Kẻ địch chính | Skill mở khóa | Tuyệt Kỳ mở khóa |
|-----|--------|---------------|---------------|------------------|
| **1** | Rừng Thiêng — Vượt Thử Thách | Cọc nhọn, hố sâu, 1 rắn tuần tra | ⚔️ Phi Rìu `[1]` + 🌪️ Lốc Xoáy `[2]` | 🪓 J + 🌀 K |
| **2** | Hang Đá Đầu Biển — Hỗn Chiến | Bầy rắn nhỏ + Bầy đại bàng | 💥 Sơn Hà Thiên Tổ `[3]` | ⚡ L |
| **3** | Đáy Hầm Tối — Đại Xà Canh Giữ | Đại Xà + bầy quái nhỏ spawn liên tục | *(Không thêm — dùng toàn bộ để đánh Đại Xà)* | — |
| **4** | Sào Huyệt Chằn Tinh — Quyết Chiến | Chằn Tinh (3 giai đoạn) + Giải cứu Công chúa | — | — |

---

## 🌟 HỆ THỐNG KỸ NĂNG THƯỜNG

> Ba kỹ năng của Thạch Sanh — mở khóa dần theo từng màn, dùng được tự do trong chiến đấu.

| Slot | Phím | Tên | Mô tả | Cooldown | Mở khóa |
|---|---|---|---|---|---|
| 1 | `1` | **Phi Rìu Thần** | Ném rìu tự động truy theo kẻ địch gần nhất (500px), xuyên lửa thần | 4 giây | Sau Màn 1 |
| 2 | `2` | **Lốc Xoáy Phong Thần** | Xoay rìu 3 giây, tạo cột bão 5 luồng xoắn ốc, sát thương liên tục, bán kính 130px | 6 giây | Sau Màn 1 |
| 3 | `3` | **Sơn Hà Thiên Tổ** | Nhảy cao 180px → Freeze Frame → Giáng rìu địa chấn ATK×6, vùng 350px | 20 giây | Sau Màn 2 |

---

## 🏆 HỆ THỐNG TUYỆT KỲ — PHẦN THƯỞNG CUỐI MÀN

> **Tuyệt Kỳ** xuất hiện như phần thưởng đặc biệt cuối mỗi màn — do **Ngọc Hoàng** ban thưởng.
> Cần **100% Năng Lượng** và chỉ dùng được khi Boss **mất phòng thủ (choáng / vỡ giáp)**.

---

### 🪓 TUYỆT KỲ J — PHI RÌU THẦN *(Mở sau Màn 1)*
**Phím:** `J` | **Năng lượng:** 100%

| Thuộc tính | Giá trị |
|---|---|
| Sát thương | **500%** |
| Hiệu ứng | Choáng **3 giây** |
| Đặc biệt | Xuyên thủng nhiều kẻ địch trên đường bay |
| Tầm bắn | Tối đa **10m** — tự nhắm kẻ mạnh nhất |

> 💡 Giữ `J` để chắm hướng. Phi rìu thần sẽ xuyên thủng mọi kẻ địch trên đường.

---

### 🌀 TUYỆT KỲ K — TRUY PHONG TUYỆT ĐỈNH RÌU *(Mở sau Màn 1)*
**Phím:** `K` | **Năng lượng:** 100%

| Thuộc tính | Giá trị |
|---|---|
| Sát thương | **300%** |
| Hiệu ứng | Đỡ đòn & Phản đòn toàn diện |
| Phạm vi | Quay rìu quét toàn bộ kẻ địch xung quanh |

> 💡 Nhấn `K` khi bị dồn để ngắt đòn, phòng thủ rồi phản công đẩy lùi mọi kẻ đến gần.

---

### ⚡ TUYỆT KỲ L — THẠCH SANH GIÁNG THỦY — PHỤC XÀ THẦN *(Mở sau Màn 2)*
**Phím:** `L` | **Năng lượng:** 100%

| Thuộc tính | Giá trị |
|---|---|
| Sát thương | **300%** |
| Hiệu ứng | Choáng **2 giây** — **Không thể né tránh** |
| Điều kiện | Chỉ kích hoạt khi Boss mất phòng thủ |

> 💡 Chờ Boss tung chiêu lớn hoặc khi lớp giáp bị phá — kích hoạt `L` để kết liễu.
> *"Chiếc rìu thần truyền sức mạnh từ đất trời. Mọi con xà yêu đều không thể chống đỡ."*

---
---

# 🌲 MÀN 1 — RỪNG THIÊNG: BÀI THỬ ĐẦU TIÊN

## 🎬 Cutscene Mở Đầu

**[Rừng cây xanh thẫm. Hoàng hôn yếu ớt. Thạch Sanh một mình dưới gốc đa]**

> 🗣️ **Thạch Sanh:** *"Chằn Tinh đã bắt công chúa vào hang tối. Không ai dám bước vào… Nhưng ta không thể ngồi yên. Rìu thần trên tay — ta đi thôi!"*

**[Thạch Sanh bước vào rừng — không có bóng quái nào, chỉ có thiên nhiên hiểm trở phía trước]**

---

## 🎮 Gameplay — Vượt Chướng Ngại Vật

> Màn học việc — **không có kẻ thù chiến đấu**. Chỉ có bẫy tự nhiên và thiên nhiên.

| Khu vực | Chướng ngại | Hướng dẫn |
|---------|------------|----------|
| **Khu 1** — Đồng cỏ bằng phẳng | Địa hình đơn giản | Di chuyển cơ bản `← →` |
| **Khu 2** — Hố sâu đầu tiên | Hố tử thần rộng 200px | Nhảy qua `SPACE` |
| **Khu 3** — Vách đá cao | Hố rộng 230px + độ cao | Double Jump `SPACE × 2` |
| **Khu 4** — Cọc nhọn nhô lên | Cọc gỗ thò lên thụt xuống theo nhịp | Quan sát nhịp — chạy qua đúng lúc |
| **Khu 5** — Hành lang cọc dày | Cọc nhọn dày đặc, khoảng hở hẹp | Nhảy chính xác + timing |
| **Khu 6** — Hố + cọc kết hợp | Hố sâu + cọc hai bên tường | Kỹ năng tổng hợp |

> 💬 *"Cẩn thận những cọc nhọn! Quan sát nhịp lên xuống trước khi chạy qua."*

> 💬 *"Nhấn [SPACE] lần 2 giữa không trung để nhảy thêm một lần nữa — Double Jump!"*

---

## 🐍 Kẻ Địch Duy Nhất — Rắn Tuần Tra Cửa Hang

**[Cuối khu 6 — cửa hang động tối tăm. Một con rắn lớn đang tuần tra qua lại chắn đường]**

> 💬 *"Trước mặt là cửa hang — nhưng có một con rắn đang canh giữ. Phải hạ nó mới vào được!"*

> 🗣️ **Rắn tuần tra** *(ngẩng đầu lên, phát hiện Thạch Sanh)*: *"SSSSS… Kẻ nào dám bén mảng đến đây?"*

> 🗣️ **Thạch Sanh:** *"Tránh ra! Ta phải vào hang đó."*

**[Giao chiến — đây là lần đầu người chơi học cách đánh combo cận chiến cơ bản]**

> 💬 *"Dùng tổ hợp tấn công để đánh combo! Chú ý lùi lại khi rắn sắp cắn."*

**[Rắn ngã xuống — cửa hang mở ra]**

---

## 🔮 Phần Thưởng Cuối Màn 1 — Phiến Đá Thiên Đình

**[Bên trong cửa hang — ánh vàng rực lên từ một phiến đá cổ khắc chữ Thiên Đình]**

> 🗣️ **Thạch Sanh** *(đặt tay lên phiến đá, rung chuyển nhẹ)*: *"Phiến đá cổ… có chữ khắc từ Thiên Đình!"*

**[Tiếng vang từ trên cao — giọng uy nghiêm trầm hùng]**

> 🗣️ **Ngọc Hoàng:** *"Thạch Sanh! Ta đang dõi theo hành trình của ngươi. Ngươi đã vượt qua rừng thiêng bằng ý chí — không hề nản lòng dù hiểm nguy. Rìu thần của ngươi xứng đáng được thức tỉnh! Hãy nhận lấy hai pháp thuật đầu tiên — dùng chúng bảo vệ lẽ phải trên con đường phía trước!"*

**[Rìu thần phát sáng — hai luồng ánh sáng xanh và vàng bao quanh Thạch Sanh]**

> 🗣️ **Thạch Sanh** *(nhìn vào hai lòng bàn tay, cảm nhận sức mạnh)*: *"Hai pháp thuật từ Thiên Đình… Ngọc Hoàng đang phù hộ ta. Ta sẽ không phụ lòng người!"*

### ✅ MỞ KHÓA SAU MÀN 1:

**⚔️ KỸ NĂNG 1 — PHI RÌU THẦN `[Phím 1]`**
> *"Ném rìu homing — tự bay truy theo kẻ thù gần nhất. Rìu xoay tít, bao quanh lửa thần cam rực."*
> 📖 *Trong truyện cổ tích: Thạch Sanh có tài ném rìu bắn chim từ xa — sức mạnh thiên bẩm do Ngọc Hoàng ban từ khi mới sinh.*

**🌪️ KỸ NĂNG 2 — LỐC XOÁY PHONG THẦN `[Phím 2]`**
> *"Xoay người 3 giây, tạo cột bão 5 luồng xoắn ốc — sát thương liên tục xung quanh, đẩy kẻ thù ra xa."*
> 📖 *Thạch Sanh sống dưới gốc đa tụ tập thần linh — gió trời ủng hộ người có lòng ngay thẳng.*

**🪓 TUYỆT KỲ J — PHI RÌU THẦN** *(phiên bản tối thượng — 500% sát thương, xuyên đám)*

**🌀 TUYỆT KỲ K — TRUY PHONG TUYỆT ĐỈNH RÌU** *(đỡ đòn + phản đòn toàn diện)*

---

## 🎬 Kết Màn 1

> 🗣️ **Thạch Sanh** *(nhìn vào bóng tối phía trong hang)*: *"Rìu thần có thể bay xa — Lốc xoáy có thể đẩy bầy quái ra. Ngọc Hoàng đang ở bên ta. Tiến thôi — Chằn Tinh, ta đến!"*

> 📜 *"Vượt qua rừng thiêng, Thạch Sanh bước vào vùng hang đá đầu biển — nơi bầy lính của Chằn Tinh đang chờ đợi…"*

---
---

# 🌊 MÀN 2 — HANG ĐÁ ĐẦU BIỂN: HỖN CHIẾN HAI TẦNG

## 🎬 Cutscene Mở Đầu

**[Hang đá ven biển. Âm thanh sóng vỗ + tiếng rít của rắn + tiếng cánh đập của đại bàng vang vọng]**

> 🗣️ **Thạch Sanh** *(nhìn quanh, hít thở)*: *"Cả rắn lẫn đại bàng — chúng bố trí cả trên cao lẫn dưới thấp. Lính canh của Chằn Tinh không đơn giản…"*

**[Tiếng gào từ bóng tối — một con rắn đỏ lao ra]**

> 🗣️ **Rắn đỏ đầu đàn:** *"Kẻ xâm nhập! Bầy anh em — xé nát hắn ra! Đại bàng — lao xuống từ trên!"*

> 🗣️ **Thạch Sanh** *(giọng kiên quyết, rìu sáng lên)*: *"Cùng lúc từ hai hướng ư? Được rồi — ta có đủ pháp thuật cho cả hai!"*

---

## 🎮 Gameplay — Hỗn Chiến 2 Tầng

> **Cơ chế đặc biệt:** Rắn bò lên từ đất, đại bàng lao xuống từ trần hang. Người chơi **không thể đứng yên**.

### Tầng 1 — Mặt Đất (Bầy Rắn Nhỏ):

| Loại rắn | Đặc điểm | Cách xử lý |
|---|---|---|
| **Rắn xanh nhanh** | Lao thẳng về người chơi, ít máu | Đánh combo cận chiến |
| **Rắn đỏ phun độc** | Phun nọc từ xa, né gần | Dùng **Phi Rìu `[1]`** tiêu diệt từ xa |
| **Rắn đất cuộn giáp** | Có giáp khi cuộn tròn | Chờ nó duỗi ra rồi mới tấn công |

### Tầng 2 — Trên Không (Bầy Đại Bàng):

| Loại đại bàng | Đặc điểm | Cách xử lý |
|---|---|---|
| **Đại bàng trinh sát** | Bay vòng, gọi thêm đồng đội | Hạ trước bằng **Phi Rìu `[1]`** |
| **Đại bàng lao công** | Bổ nhào từ trên với tốc độ cao | Né sang bên → phản công khi nó đáp đất |
| **Đại bàng đen (lớn)** | Bắt nhân vật nếu đứng yên quá 2 giây | **Liên tục di chuyển!** |

### Làn Sóng Tấn Công:

| Làn sóng | Nội dung | Gợi ý chiến thuật |
|---------|---------|-----------------|
| **Sóng 1** | 3 rắn xanh đơn thuần | Làm quen với combo + Phi Rìu |
| **Sóng 2** | 2 rắn đỏ + 2 đại bàng trinh sát | Phi Rìu bắn đại bàng, đánh rắn cận chiến |
| **Sóng 3** | 4 rắn hỗn hợp + 3 đại bàng lao công | Vừa né vừa phản công — tempo dữ dội |
| **Sóng 4** — BAO VÂY | Cả bầy từ mọi hướng | **→ Dùng Lốc Xoáy `[2]`** để quét sạch! |

> 💬 *"Lốc Xoáy sẽ đẩy cả rắn lẫn đại bàng ra xa cùng lúc — hoàn hảo khi bị vây!"*

> 💬 *"Rắn đỏ phun độc từ xa — hãy dùng [1] để Phi Rìu tự bay tìm đến!"*

---

## 🏁 Boss Cuối Màn 2 — Đại Bàng Vương + Rắn Chúa

**[Phòng rộng lớn — từ trên cao, một con đại bàng khổng lồ đáp xuống. Cùng lúc, tiếng động dưới đất…]**

> 🗣️ **Đại Bàng Vương:** *"Ngươi dám đi đến tận đây?! Ta — Đại Bàng Vương — sẽ không để ngươi qua!"*

> 🗣️ **Thạch Sanh:** *"Đại Bàng Vương — dẹp đường! Chằn Tinh là kẻ ta cần gặp."*

**Cơ chế chiến đấu:**
- Đại Bàng Vương lượn trên cao, **thả bầy rắn nhỏ** xuống liên tục
- Khi Đại Bàng Vương mất 50% máu → **Rắn Chúa Đôi Đầu** trồi lên từ hố đất
- **Chiêu kết hợp nguy hiểm:** Đại Bàng bắt người chơi lên cao → Rắn Chúa há miệng đón dưới

> 💬 *"Đại Bàng Vương thả rắn xuống — dùng Lốc Xoáy để quét sạch bầy rắn trước!"*

> 💬 *"Khi Rắn Chúa xuất hiện, tập trung hạ Đại Bàng Vương trước — nếu không bị bắt lên cao sẽ rất nguy hiểm!"*

**[Đại Bàng Vương ngã xuống → Rắn Chúa mất hỗ trợ → Hạ nốt]**

> 🗣️ **Đại Bàng Vương** *(giọng đứt quãng)*: *"Không thể ngờ… Ngươi mạnh hơn những gì Chằn Tinh nói…"*

> 🗣️ **Thạch Sanh:** *"Dẫn đường tôi đến hang Chằn Tinh đi — đó là điều duy nhất ngươi có thể làm lúc này."*

---

## 🔮 Phần Thưởng Cuối Màn 2 — Ấn Đá Thiên Đình

**[Một phiến đá khắc hình chiếc rìu đang đập xuống đất — sáng rực giữa phòng tối]**

> 🗣️ **Thạch Sanh** *(đặt tay lên, cảm nhận rung chuyển mãnh liệt)*: *"Lại là phiến đá của Thiên Đình… Ngọc Hoàng còn ban thêm sức mạnh cho ta?"*

**[Tiếng Ngọc Hoàng vang vọng từ xa — uy nghiêm hơn lần trước]**

> 🗣️ **Ngọc Hoàng:** *"Thạch Sanh! Ngươi vừa một mình đương đầu với cả bầy rắn lẫn bầy đại bàng. Trời đất cảm phục sự kiên cường đó. Nhưng phía trước còn một thử thách lớn hơn — một đại xà canh giữ cổng vào hang Chằn Tinh. Hãy nhận lấy binh pháp cuối cùng: hóa thân thành rìu thần, nhảy từ trời cao giáng xuống — Sơn Hà Thiên Tổ! Và đây — Tuyệt Kỳ thứ ba, chiêu thức bất khả kháng — ta ban cho ngươi để hạ con xà yêu kia!"*

**[Ánh sáng trắng bùng lên — cả ba luồng sức mạnh bao quanh Thạch Sanh]**

> 🗣️ **Thạch Sanh** *(giọng đầy quyết tâm)*: *"Sơn Hà Thiên Tổ… Tuyệt Kỳ Phục Xà Thần… Ngọc Hoàng đã trao cho ta đủ sức mạnh. Đại Xà — ta đến!"*

### ✅ MỞ KHÓA SAU MÀN 2:

**💥 KỸ NĂNG 3 — SƠN HÀ THIÊN TỔ `[Phím 3]`**
> *"Nhảy vọt 180px → Freeze Frame thế giới chậm lại → Giáng rìu địa chấn ATK×6, vùng 350px, đất nứt vàng lan rộng."*
> 📖 *Thạch Sanh nhảy xuống hang cứu Công chúa — hành động xả thân không tính toán. Đòn này tượng trưng lẽ phải trời đất đè bẹp cái ác.*

**⚡ TUYỆT KỲ L — THẠCH SANH GIÁNG THỦY - PHỤC XÀ THẦN** *(300%, không thể né — dành cho Đại Xà)*

---

## 🎬 Kết Màn 2

> 🗣️ **Thạch Sanh** *(bước về phía lối đi sâu hơn)*: *"Bầy rắn, bầy đại bàng — tất cả đã bị dẹp. Tiếng gầm từ phía trong… là tiếng của một thứ to hơn nhiều. Nhưng ta đã có đủ sức mạnh rồi."*

> 📜 *"Xuyên qua hang đá đầu biển, Thạch Sanh tiến vào đáy hầm tối — nơi Đại Xà khổng lồ đang canh giữ cổng vào hang Chằn Tinh…"*

---
---

# 🐍 MÀN 3 — ĐÁY HẦM TỐI: ĐẠI XÀ CANH GIỮCỔNG

## 🎬 Cutscene Mở Đầu

**[Đáy hầm tối tăm. Nhũ đá. Nước nhỏ giọt. Bóng tối dày đặc — rồi… một cặp mắt khổng lồ mở ra trong bóng tối]**

> 🗣️ **Thạch Sanh** *(dừng lại, nhìn vào bóng tối, tay siết rìu)*: *"Cái gì đó rất lớn đang ở trong đó…"*

**[Đại Xà từ từ trườn ra — thân hình khổng lồ chiếm 40% chiều ngang màn hình, vảy xanh đen lấp lánh]**

> 🗣️ **Đại Xà** *(tiếng rít như sấm)*: *"SSSSSS… Ngươi đã hạ được lính canh của ta. Nhưng đây là ranh giới. Không ai qua được ta để gặp chủ nhân — Chằn Tinh vĩ đại!"*

> 🗣️ **Thạch Sanh** *(không lùi)*: *"Ta không hỏi. Ta chỉ đi qua."*

> 🗣️ **Đại Xà:** *"Vậy thì… chết đi!"*

---

## 🎮 Gameplay — Đồng Thời Đối Phó Đại Xà + Bầy Quái Nhỏ

> **Cơ chế:** Trong khi đánh Đại Xà, bầy quái nhỏ liên tục spawn mỗi 15 giây từ các khe đá.

### 👾 Bầy Quái Nhỏ (Spawn Liên Tục):

| Loại quái | Đặc điểm | Xử lý |
|---|---|---|
| **Rắn đất nhỏ** | Nhanh, ít máu, xuất hiện theo bầy | Dùng **Lốc Xoáy `[2]`** quét cả bầy |
| **Ma lửa nhỏ** | Lơ lửng, phun lửa cố định | **Phi Rìu `[1]`** hạ từ xa |
| **Nhện hang** | Thả tơ cản đường, làm chậm nhân vật | Né tơ + tiêu diệt ngay |

> 💬 *"Đừng để bầy quái nhỏ dồn lại — chúng sẽ cản trở khi đang đánh Đại Xà!"*

### 🏁 Đại Xà — Bộ Chiêu Thức:

| Chiêu | Mô tả | Cảnh báo | Cách phản công |
|---|---|---|---|
| 🐍 **Phun nọc độc** | Luồng độc theo đường thẳng | Màu xanh lá bùng lên trước miệng | Nhảy né hoặc lăn sang ngang |
| 🐍 **Siết vòng** | Cuộn thân tạo vòng tròn sát thương | Thân rắn bắt đầu cuộn tròn chậm | Đứng vào điểm an toàn ở giữa |
| 🐍 **Đập đuôi** | Vẫy đuôi quét toàn sàn ngang | Đuôi phát sáng đỏ | **Nhảy lên né → Phản công đầu rắn ngay!** |
| 🐍 **Há miệng lao** | Lao đầu thẳng về phía nhân vật | Miệng mở rộng + gầm to | **→ Cơ hội lớn: Né sang, rắn trượt → Choáng 3s!** |

### ⚡ Cơ Chế Phá Giáp — Khi Há Miệng Lao Trượt:

> **[Đại Xà há miệng lao → Người chơi né → Rắn đập đầu vào đá → Choáng 3 giây]**

> 💬 *"Bây giờ! Đại Xà đang choáng — kích hoạt Tuyệt Kỳ L để kết liễu!"*

> 🗣️ **Thạch Sanh** *(nhảy lên, rìu thần phát sáng trắng)*: *"Ngọc Hoàng đã ban cho ta — THẠCH SANH GIÁNG THỦY PHỤC XÀ THẦN!!!"*

**[Rìu giáng xuống đầu Đại Xà — không thể né — choáng 2 giây — 300% sát thương]**

**[Đại Xà ngã xuống, thân hình khổng lồ tan dần thành ánh sáng xanh]**

---

## 🎬 Cutscene Kết Màn 3

> 🗣️ **Đại Xà** *(giọng yếu ớt dần)*: *"Không… tưởng được… Tuyệt Kỳ của Thiên Đình… ta… không thể… chống đỡ…"*

> 🗣️ **Thạch Sanh** *(nhìn về phía cửa hang mở ra)*: *"Xà thần đã bị hạ. Cánh cửa đã mở. Chằn Tinh — ta có thể nghe thấy tiếng gõ nhẹ của một chiếc lồng sắt từ trong đó… Công chúa vẫn còn ở đó."*

**[Ánh sáng le lói từ phía trong — tiếng gõ nhẹ khẽ vang]**

> 📜 *"Đại Xà ngã xuống, thân hình tan thành ánh sáng. Cổng vào hang Chằn Tinh mở ra. Thạch Sanh bước vào — đây là trận chiến cuối cùng…"*

---
---

# 🏯 MÀN 4 — SÀOHUYỆT CHẰN TINH: QUYẾT CHIẾN VÀ GIẢI CỨU

## 🎬 Cutscene Mở Đầu

**[Hang rộng lớn hoành tráng. Ánh lửa đỏ rực từ hai bên tường. Giữa không trung — một chiếc lồng sắt treo lơ lửng bằng xích. Bên trong… Công chúa Quỳnh Nga]**

> 🗣️ **Công chúa Quỳnh Nga** *(tiếng gọi thảng thốt từ trong lồng)*: *"Ai đó… cứu tôi! Ai ở đó không?!"*

> 🗣️ **Thạch Sanh** *(ngước nhìn lồng sắt, giọng dịu lại)*: *"Công chúa — ta đến rồi. Hãy chờ ta một lúc nữa thôi!"*

**[Mặt đất rung chuyển — Chằn Tinh xuất hiện từ bóng tối phía sau. Thân hình khổng lồ, mắt đỏ rực như than hồng]**

> 🗣️ **Chằn Tinh** *(tiếng gầm như sấm rền)*: *"THẠCH SANH!!! Ngươi thật sự đến được tận đây?! Ta phải thừa nhận — ngươi đã hạ được tất cả lính canh của ta. Nhưng đây là sào huyệt của ta — ngươi sẽ không bao giờ thoát ra!"*

> 🗣️ **Thạch Sanh** *(giơ rìu lên, ánh sáng ba Tuyệt Kỳ lấp lánh trên thân rìu)*: *"Chằn Tinh — ngươi bắt cóc người vô tội, gieo rắc nỗi sợ hãi cho muôn dân. Hôm nay ta mang lẽ phải của trời đất đến đây kết thúc tất cả. Ba binh pháp, ba Tuyệt Kỳ — tất cả đều dành cho ngươi!"*

> 🗣️ **Công chúa** *(từ trong lồng, lo lắng)*: *"Thạch Sanh… cẩn thận! Chằn Tinh rất mạnh!"*

> 🗣️ **Thạch Sanh** *(nhìn lên lồng sắt, mỉm cười)*: *"Ta biết. Nhưng ta còn mạnh hơn."*

---

## ⚔️ BOSS FIGHT — CHẰN TINH 3 GIAI ĐOẠN

---

### 🔴 GIAI ĐOẠN 1 — "CHẰN TINH PHÒNG THỦ" (100% → 66% HP)
*Lớp giáp đá bao phủ toàn thân — đòn thường bật ra*

| Chiêu | Mô tả | Cách né + phản công |
|---|---|---|
| **Đấm Đất** | Đập tay xuống tạo sóng chấn địa lan 2 hướng | Nhảy lên khi sóng chấn địa đến |
| **Hú Triệu Tập** | Gọi 3 rắn nhỏ xuất hiện từ bên hông | **→ Dùng Tuyệt Kỳ K** quét sạch rắn + đánh vào giáp cùng lúc |
| **Lăn Phình** | Phình to rồi lăn toàn sàn về phía nhân vật | Nhảy lên nền tảng cao, né đường lăn |

> 💬 *"Giáp đá của Chằn Tinh rất dày! Chờ nó Hú để triệu rắn — dùng Tuyệt Kỳ K phá giáp và quét rắn cùng lúc!"*

**[Sau khi giáp vỡ]**

> 🗣️ **Chằn Tinh** *(giáp đá vỡ tan, gầm lên tức giận)*: *"NGỪ ĐÁ!!! Ngươi dám phá giáp của ta?! TA SẼ NGHIỀN NAT NGƯƠI!!"*

> 🗣️ **Thạch Sanh** *(không nao núng)*: *"Lớp vỏ đã bị phá. Bây giờ mới đến phần thật sự."*

---

### 🟠 GIAI ĐOẠN 2 — "CHẰN TINH CUỒNG NỘ" (66% → 33% HP)
*Giáp vỡ → Tốc độ tăng, tấn công dữ dội hơn*

| Chiêu | Mô tả | Cách né + phản công |
|---|---|---|
| **Tát Bay** | Tát ngang cực nhanh, tầm rất xa | Lăn vào gần người hoặc lùi xa hẳn |
| **Phun Lửa Địa Ngục** | Phun 3 luồng lửa hướng ngẫu nhiên | Né liên tục — sau đó có 2s khoảng hở! |
| **Đạp Chân Liên Tục** | Đạp liên tục rung mặt đất 3 giây | Nhảy liên tục để tránh chấn động |
| **Giả Vờ Ngã** | Té xuống → khi lại gần thì bật dậy phản công | Không được lại gần khi Chằn Tinh ngã! |

> 💬 *"Sau khi Chằn Tinh phun lửa xong, nó bị mệt 2 giây — đây là cơ hội vàng! Dùng Tuyệt Kỳ J!"*

> 🗣️ **Chằn Tinh** *(gầm rú)*: *"THẠCH SANH! Ngươi làm ta TỨC GIẬN! Ta sẽ phá nát ngươi thành từng mảnh!"*

> 🗣️ **Thạch Sanh** *(né liên tục, thở gấp nhưng vẫn tập trung)*: *"Càng tức giận — càng sơ hở. Chờ đi, Chằn Tinh!"*

**[Chằn Tinh phun lửa xong, đứng nghiêng khò hơi 2 giây → Thạch Sanh kích hoạt Tuyệt Kỳ J]**

**[Phi Rìu Thần xuyên thẳng vào ngực Chằn Tinh → 500% sát thương → Choáng 3 giây]**

> 🗣️ **Công chúa** *(từ trong lồng, hét lên)*: *"Thạch Sanh! Mạnh lắm!"*

---

### 🔵 GIAI ĐOẠN 3 — "CHẰN TINH TÀN CUỘC" (33% → 0%)
*Điên loạn hoàn toàn — bùng phát sức mạnh tối thượng*

**[Chằn Tinh máu cạn kiệt — thân mình bốc khói, mắt đỏ rực nhất từ trước đến nay]**

> 🗣️ **Chằn Tinh** *(tiếng rống sơn hà)*: *"TA… KHÔNG… THUA!!! TA LÀ CHẰN TINH VĨ ĐẠI!!! THẠCH SANH — NGƯƠI SẼ CÙNG TA XUỐNG ĐÁY ĐẤT!!!"*

| Chiêu | Mô tả | Cách xử lý |
|---|---|---|
| **Toàn Thân Lửa Độc** | Cơ thể bao phủ lửa — chạm vào mất máu liên tục | **Không chạm người Chằn Tinh — chỉ đánh từ xa** |
| **Đập Ba Liên** | Đập tay 3 lần liên tiếp, tốc độ tăng dần | Né + lăn liên tục 3 lần |
| **Hú Điên Loạn** | Rung màn hình + gọi toàn bộ quái còn lại | Dùng **Sơn Hà Thiên Tổ `[3]`** quét sạch |
| **Lao Thân** | Nhắm thẳng vào người chơi, lao tốc độ tối đa | **→ Đây là cơ hội cuối cùng!** |

### 🎯 Cú Đánh Kết Thúc — Tuyệt Kỳ Finisher:

> 🗣️ **Thạch Sanh** *(né Lao Thân, Chằn Tinh trượt đập vào tường)*: *"Ngươi đã thua rồi, Chằn Tinh. Không phải vì ta mạnh hơn — mà vì lẽ phải trời đất luôn thắng kẻ hung tàn!"*

**[Chằn Tinh choáng 2 giây — mất hoàn toàn phòng thủ]**

> 💬 *"BÂY GIỜ! Chằn Tinh đang choáng — kích hoạt Tuyệt Kỳ L để kết liễu!"*

> 🗣️ **Thạch Sanh** *(giọng hét xé trời, rìu thần phát sáng trắng rực)*:
> *"Ngọc Hoàng ban lực — đất trời hội tụ —*
> ***THẠCH SANH GIÁNG THỦY — PHỤC XÀ THẦN!!!***"

**[Màn hình flash trắng. Rìu thần giáng xuống đầu Chằn Tinh — 300% sát thương — không thể né — choáng 2 giây]**

**[Chằn Tinh ngã xuống — thân hình khổng lồ rung chuyển cả hang]**

---

## 🎬 CUTSCENE GIẢI CỨU CÔNG CHÚA

**[Màn hình chậm lại]**

> 🗣️ **Chằn Tinh** *(giọng tan biến dần)*: *"Không thể… Tuyệt Kỳ của Thiên Đình… ta đã kiêu ngạo… quá lâu…"*

**[Chằn Tinh tan thành những mảnh đá vỡ — bụi mù bay lên. Hang đá rung chuyển. Xích lồng sắt văng ra]**

> 🗣️ **Thạch Sanh** *(chạy đến dưới lồng sắt, một nhát rìu — xích đứt tung)*:

**[Lồng sắt hạ xuống — Công chúa Quỳnh Nga bước ra, mắt đỏ hoe, chân còn run]**

> 🗣️ **Công chúa Quỳnh Nga** *(run run)*: *"Chàng… thật sự đã đến. Ta cứ tưởng không ai dám vào đây… Chằn Tinh nói không ai có thể hạ được nó…"*

> 🗣️ **Thạch Sanh** *(mỉm cười nhẹ nhàng)*: *"Người vô tội không nên bị giam cầm. Đó là lý do duy nhất ta vào đây. Không có gì cao cả hơn thế."*

> 🗣️ **Công chúa** *(rơi nước mắt)*: *"Cảm ơn chàng, Thạch Sanh. Cảm ơn chàng rất nhiều."*

**[Tiếng Ngọc Hoàng vang lên lần cuối — ấm áp hơn, không còn uy nghiêm như trước]**

> 🗣️ **Ngọc Hoàng:** *"Thạch Sanh — ta đã dõi theo từng bước chân của ngươi. Từ gốc đa nghèo khó đến sào huyệt Chằn Tinh — ngươi không một lần do dự, không một lần toan tính. Đó chính là lý do trời đất ban sức mạnh cho ngươi. Hãy trở về — ngươi xứng đáng được hưởng hạnh phúc."*

**[Ánh bình minh tràn vào hang. Lối ra xuất hiện phía trước. Thạch Sanh và Công chúa cùng bước về phía ánh sáng]**

---

## 📜 MÀN HÌNH CHIẾN THẮNG

> *"Thạch Sanh mang Công chúa Quỳnh Nga trở về Vương Quốc bình an.*
>
> *Nhà vua mừng mừng tủi tủi, ban thưởng trọng hậu — Thạch Sanh được phong Phò Mã.*
>
> *Nhưng điều chàng mang về thực sự không phải vinh hoa hay địa vị —*
> ***mà là bài học ngàn đời:***
>
> *Lòng dũng cảm không biết sợ,*
> *tình yêu trong sáng không tính toán,*
> *lẽ phải chính nghĩa không bao giờ cúi đầu —*
> ***sẽ luôn chiến thắng mọi cái ác trên đời.***
>
> *— Thạch Sanh, truyện cổ tích Việt Nam —"*

---
---

## 📊 TỔNG KẾT HỆ THỐNG MỞ KHÓA

```
════════════════════════════════════════
  THẠCH SANH BẮT ĐẦU
  — Chỉ có tấn công thường (rìu cận chiến)
════════════════════════════════════════

⏬ HOÀN THÀNH MÀN 1 (Rừng Thiêng)
    ↳ Ngọc Hoàng ban thưởng:
       ⚔️  Kỹ năng 1: PHI RÌU THẦN [1]  ← 4s cooldown
       🌪️  Kỹ năng 2: LỐC XOÁY PHONG THẦN [2]  ← 6s cooldown
       🪓  Tuyệt Kỳ J: PHI RÌU THẦN (500%, choáng 3s, xuyên đám)
       🌀  Tuyệt Kỳ K: TRUY PHONG TUYỆT ĐỈNH RÌU (đỡ+phản đòn)

⏬ HOÀN THÀNH MÀN 2 (Hang Đá Đầu Biển)
    ↳ Ngọc Hoàng ban thưởng:
       💥  Kỹ năng 3: SƠN HÀ THIÊN TỔ [3]  ← 20s cooldown
       ⚡  Tuyệt Kỳ L: PHỤC XÀ THẦN (300%, không thể né, choáng 2s)

⏬ VÀO MÀN 3 (Đáy Hầm Tối)
    ↳ Dùng toàn bộ sức mạnh để hạ Đại Xà

⏬ VÀO MÀN 4 (Sào Huyệt Chằn Tinh)
    ↳ Trận quyết chiến — 3 giai đoạn
    ↳ Giải cứu Công chúa Quỳnh Nga

════════════════════════════════════════
  KẾT THÚC — WIN SCREEN
════════════════════════════════════════
```

---

## 🎼 BỐI CẢNH VÀ ÂM NHẠC

| Màn | Nhạc Nền | Bối Cảnh Hình Ảnh |
|---|---|---|
| **Màn 1** | Nhẹ nhàng, tiếng gió rừng, nhạc dân tộc | Rừng cây xanh, cây đa, hoàng hôn |
| **Màn 2** | Dồn dập, căng thẳng, trống trận | Hang đá ven biển, ánh lửa mờ, sóng vỗ |
| **Màn 3** | U ám, huyền bí, tiếng rắn rít xa | Đáy hầm tối, nhũ đá, nước nhỏ giọt |
| **Màn 4** | Sử thi hoành tráng, boss music rầm rộ | Hang rộng, lửa đỏ hai bên, lồng sắt treo |

---

*📝 Kịch bản hoàn chỉnh v2.0 — Game Thạch Sanh 2D*
*Cập nhật: 11/03/2026*
