# Quy tắc lập trình Game 3D (Unity)

## 1. Kiến trúc & Cấu trúc dự án
- Tách rõ **logic game** (gameplay, state) khỏi **logic render** (hiển thị, hiệu ứng). Tránh nhét xử lý game vào trong code render hoặc ngược lại.
- Dùng pattern **Entity-Component** (hoặc ECS) thay vì kế thừa sâu (deep inheritance) để dễ mở rộng object.
- Tổ chức thư mục theo tính năng (feature-based) thay vì theo loại file (không gom hết script vào 1 thư mục `Scripts`).
- Mỗi hệ thống (di chuyển, chiến đấu, AI, inventory...) nên là module độc lập, giao tiếp qua event/message (C# event, `UnityEvent`, hoặc ScriptableObject Event Channel) thay vì gọi trực tiếp chồng chéo lẫn nhau.
- Hạn chế Singleton/`static` tràn lan cho Manager; nếu dùng, cân nhắc DI đơn giản hoặc ScriptableObject làm "service locator" để dễ test và tránh phụ thuộc chéo giữa scene.
- Cân nhắc dùng **DOTS/ECS (Entities, Burst, Job System)** cho phần cần số lượng object lớn (hàng ngàn unit, particle logic...), còn lại dùng GameObject/MonoBehaviour truyền thống cho gameplay thường.

## 2. Hiệu năng (Performance)
- **Object Pooling**: không Instantiate/Destroy liên tục (đạn, hiệu ứng, kẻ địch) — tái sử dụng object có sẵn.
- **Batching**: gộp draw call bằng Static Batching (object tĩnh), GPU Instancing/SRP Batcher cho object lặp lại (cây, đá, cỏ...). Kiểm tra render pipeline đang dùng (Built-in/URP/HDRP) vì cách tối ưu khác nhau khá nhiều.
- **LOD Group**: dùng component `LODGroup` để giảm chi tiết model/texture theo khoảng cách camera.
- **Occlusion Culling**: bake Occlusion Culling trong Unity (Window > Rendering > Occlusion Culling) cho scene có nhiều vật cản tầm nhìn.
- Tránh gọi `GetComponent`, `GameObject.Find`, `Camera.main` (không cache), hay `Physics.Raycast` phức tạp trong `Update()` mỗi frame — cache tham chiếu ở `Awake()/Start()`, dùng coroutine hoặc `InvokeRepeating` để giãn tần suất gọi.
- Tránh dùng `Instantiate/Destroy` liên tục — dùng Object Pool (có thể tận dụng `UnityEngine.Pool.ObjectPool<T>` có sẵn từ Unity 2021+).
- Giới hạn số lượng Realtime Light và Shadow; ưu tiên **baked lighting/lightmap** hoặc Mixed Lighting khi có thể; dùng Light Probes cho object động trong khu vực đã bake.
- Theo dõi draw calls, triangle count, memory bằng **Profiler** và **Frame Debugger** thường xuyên, đừng đợi đến cuối dự án mới tối ưu.
- Cẩn thận với `OnGUI()` (rất tốn hiệu năng) — ưu tiên UI Toolkit hoặc uGUI (Canvas) cho giao diện.
- Với Canvas UI: tách Canvas tĩnh/động riêng, tránh 1 Canvas lớn chứa cả phần tử thay đổi liên tục (gây rebuild toàn bộ layout).

## 3. Vật lý (Physics)
- Luôn xử lý vật lý (`Rigidbody.AddForce`, `MovePosition`...) trong `FixedUpdate()`, không phải `Update()`.
- Dùng collider đơn giản (Box, Sphere, Capsule) thay vì Mesh Collider phức tạp khi có thể; nếu bắt buộc dùng Mesh Collider động thì bật `Convex`.
- Tránh scale object có Rigidbody bằng giá trị âm hoặc không đều — gây lỗi vật lý khó debug.
- Set `Interpolate` cho Rigidbody cần chuyển động mượt (nhân vật chính, camera theo dõi), `Extrapolate` dùng cẩn thận vì có thể gây "xuyên vật" (tunneling) khi dự đoán sai.
- Với vật thể tốc độ cao (đạn...), bật `Continuous` hoặc `Continuous Dynamic` Collision Detection để tránh xuyên qua collider mỏng.
- Cấu hình **Layer Collision Matrix** (Physics Settings) hợp lý để giảm số cặp va chạm cần kiểm tra mỗi frame.

## 4. Camera & Input
- Dùng **Input System** package (thay cho Input Manager cũ) với Action Map để dễ đổi bàn phím/gamepad/VR sau này và hỗ trợ đa nền tảng tốt hơn.
- Camera controller nên có smoothing (`Vector3.SmoothDamp`/`Lerp` theo `Time.deltaTime`) khi follow nhân vật, tránh giật cứng; nếu cần nhanh gọn có thể dùng **Cinemachine** thay vì tự viết từ đầu.
- Xử lý va chạm camera với địa hình bằng Raycast/SphereCast giữa camera và target để camera không xuyên tường (Cinemachine có sẵn `Collider` extension cho việc này).
- Nhớ đặt logic camera trong `LateUpdate()` để chạy sau khi nhân vật đã di chuyển xong trong frame đó.

## 5. Asset & Tài nguyên
- Đặt tên asset theo quy ước rõ ràng, nhất quán (ví dụ: `SM_PropName`, `TEX_Name_Diffuse`, `ANIM_CharacterName_Action`).
- Nén texture đúng định dạng cho từng nền tảng (PC/mobile/console khác nhau).
- Kiểm tra polycount và kích thước texture phù hợp với mục tiêu hiệu năng (mobile khác PC/console); cấu hình **Texture Import Settings** đúng (Max Size, Compression) theo từng platform trong tab Platform-specific override.
- Dùng **Addressables** thay vì Resources folder cho project lớn — quản lý bộ nhớ và load/unload asset bất đồng bộ tốt hơn nhiều.
- Xóa asset không dùng định kỳ (Unity có thể check qua Console warning hoặc tool bên thứ 3 như "Unused Assets Finder"); chạy `Assets > Optimize/Analyze` hoặc kiểm tra Editor log về asset trùng lặp.

## 6. Animation
- Dùng **Animator Controller** (State Machine / Blend Tree) thay vì code chuyển animation thủ công bằng if-else.
- Truy cập tham số Animator qua `Animator.StringToHash("ParamName")` cache sẵn, tránh dùng string trực tiếp mỗi frame (tốn hiệu năng do hashing lại).
- Root Motion cần đồng bộ kỹ với `CharacterController`/`Rigidbody` di chuyển bằng code, tránh xung đột giữa animation và logic di chuyển (chọn 1 nguồn điều khiển chính).
- Animation Events nên gọi qua interface/callback rõ ràng trong script gắn cùng object, tránh gọi method ở object khác dễ lỗi khi đổi asset.
- Với nhiều nhân vật cùng loại, cân nhắc dùng **Animation Rigging** package hoặc chia sẻ chung Animator Controller/Avatar Mask để tiết kiệm bộ nhớ.

## 7. AI & Gameplay Logic
- Dùng Behavior Tree hoặc State Machine cho AI thay vì if-else lồng nhau nhiều tầng (có thể tự viết hoặc dùng asset như Behavior Designer).
- Dùng **NavMesh Agent** (`NavMeshAgent`) cho di chuyển AI; bake lại NavMesh (Window > AI > Navigation) mỗi khi thay đổi địa hình tĩnh, dùng `NavMeshSurface` (AI Navigation package) nếu cần bake runtime.
- Gameplay logic quan trọng (điểm số, máu, thắng/thua) nên có single source of truth — ví dụ đặt trong 1 ScriptableObject hoặc GameManager duy nhất, tránh đồng bộ 2 nơi dễ lệch dữ liệu.
- Cân nhắc dùng **ScriptableObject** làm data container (stats, config item, event channel) thay vì hard-code trong script — designer chỉnh được trực tiếp trên Inspector mà không cần sửa code.

## 8. Networking (nếu multiplayer)
- Xác định rõ mô hình: client-server authoritative hay peer-to-peer — ảnh hưởng toàn bộ kiến trúc.
- Trong Unity thường dùng **Netcode for GameObjects** (chính thức) hoặc **Mirror**, **Photon Fusion/PUN** tuỳ nhu cầu — chọn sớm vì khó đổi giữa chừng.
- Không tin dữ liệu từ client (validate ở server/host) để tránh cheat.
- Dùng interpolation/prediction (built-in trong `NetworkTransform` hoặc tự viết) để bù độ trễ cho chuyển động mượt.

## 9. Coding Convention (C#/Unity)
- Tên biến/hàm theo convention C#: PascalCase cho public field/method/class, camelCase cho private field/local variable (hoặc `_camelCase` cho private field tuỳ team).
- Ưu tiên `[SerializeField] private` thay vì `public` field để vừa chỉnh được trên Inspector vừa giữ encapsulation.
- Tránh magic number — dùng constant, `[Header]`/`[Tooltip]` cho field trên Inspector, hoặc ScriptableObject config để designer chỉnh không cần sửa code.
- Bọc `Debug.Log` bằng conditional compile (`#if UNITY_EDITOR` hoặc `[Conditional("DEBUG")]`) hoặc dùng logging wrapper riêng, tránh log tràn lan còn sót lại trong bản build release (rất tốn hiệu năng trên mobile).
- Cẩn thận với `[SerializeField]` bị đổi type/rename — dễ mất reference đã gán trên Inspector; dùng `[FormerlySerializedAs]` khi rename field cần giữ dữ liệu cũ.
- Tránh gọi coroutine/subscribe event mà không hủy khi object bị destroy — dùng `OnDestroy()`/`OnDisable()` để `StopCoroutine`/unsubscribe, tránh lỗi `MissingReferenceException` hoặc memory leak.

## 10. Kiểm thử & Ổn định
- Test trên thiết bị/target thật thường xuyên (đặc biệt mobile/console), không chỉ trong Editor — hiệu năng Editor không phản ánh đúng thực tế.
- Dùng **Unity Test Framework** (Edit Mode & Play Mode tests) cho các phần logic quan trọng (combat, inventory, save/load).
- Có hệ thống save/load rõ ràng (JSON/binary + `Application.persistentDataPath`), xử lý version dữ liệu khi cập nhật game sau này.
- Luôn có cơ chế xử lý lỗi (try-catch/null check, đặc biệt check `== null` cho UnityEngine.Object vì Unity override toán tử này khác C# thường) ở các điểm dễ crash: load scene (`SceneManager.LoadSceneAsync`), load save file, network request.
- Git cần `.gitignore` chuẩn của Unity (bỏ qua `Library/`, `Temp/`, `Obj/`, `Build/`...) và bật **Git LFS** cho asset nhị phân lớn (texture, model, audio); cấu hình Unity ở chế độ **Visible Meta Files** + **Force Text** (Edit > Project Settings > Editor) để merge dễ hơn.

## 11. Lưu ý riêng khi dùng AI hỗ trợ code (Anigravity)
- Luôn review kỹ code AI sinh ra trước khi áp dụng, đặc biệt phần đụng tới vật lý, hiệu năng, hoặc thay đổi cấu trúc dự án lớn.
- Cung cấp context rõ ràng (engine đang dùng, ngôn ngữ, cấu trúc project hiện tại) để AI đưa ra gợi ý phù hợp thay vì generic.
- Yêu cầu AI giải thích lý do khi đề xuất thay đổi kiến trúc, không áp dụng mù quáng.
- Kiểm tra kỹ các đoạn code liên quan tới vòng lặp `Update`/`Tick` — đây là nơi dễ phát sinh vấn đề hiệu năng nhất khi để AI viết tự động.
