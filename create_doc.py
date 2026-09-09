import os
from docx import Document
from docx.shared import Pt, Inches
from docx.enum.text import WD_ALIGN_PARAGRAPH

def add_technique(doc, name, definition, how_it_works, role):
    h = doc.add_heading(name, level=2)
    
    p1 = doc.add_paragraph()
    p1.add_run("1. Kỹ thuật này là gì?\n").bold = True
    p1.add_run(definition)
    
    p2 = doc.add_paragraph()
    p2.add_run("2. Nó hoạt động như thế nào?\n").bold = True
    p2.add_run(how_it_works)
    
    p3 = doc.add_paragraph()
    p3.add_run("3. Vai trò trong Project:\n").bold = True
    p3.add_run(role)
    
    doc.add_paragraph("_" * 50)

def create_document():
    doc = Document()
    
    # Tiêu đề chính
    title = doc.add_heading('TÀI LIỆU KỸ THUẬT: PHÂN TÍCH PROJECT GAME 2D', 0)
    title.alignment = WD_ALIGN_PARAGRAPH.CENTER
    
    doc.add_paragraph("Mục đích tài liệu: Giải thích các kỹ thuật lập trình đằng sau trò chơi một cách dễ hiểu nhất dành cho người không chuyên về lập trình, phục vụ báo cáo nhóm.\n")

    # PHẦN 1
    doc.add_heading('PHẦN 1: CÁC KỸ THUẬT LOGIC GAMEPLAY CỐT LÕI', level=1)
    
    add_technique(doc, "1. Game Loop (Vòng lặp trò chơi)",
        "Là 'nhịp tim' của bất kỳ trò chơi nào. Game Loop là một vòng lặp chạy liên tục hàng chục lần mỗi giây (thường gọi là 60 FPS - 60 khung hình/giây).",
        "Trong mỗi 1/60 giây, trò chơi làm 3 việc: Ghi nhận thao tác -> Tính toán vị trí mới của tàu và thiên thạch -> Vẽ lại hình ảnh mới lên màn hình.",
        "Đây là kỹ thuật cốt lõi tạo ra sự chuyển động mượt mà cho các vật thể thay vì đứng yên như bức ảnh tĩnh.")

    add_technique(doc, "2. Coordinate System & Vectors (Hệ Tọa Độ 2D & Chuyển động có hướng)",
        "Là hệ thống lưới ảo xác định vị trí vật thể qua trục ngang (X) và trục dọc (Y). Vector quy định hướng và tốc độ.",
        "Cộng/trừ giá trị X và Y để vật thể di chuyển (VD: cộng X để đạn bay sang phải).",
        "Giúp đặt Phi thuyền (A) chính xác ở rìa trái, Thiên thạch (B) chính xác ở rìa phải, và điều khiển đường bay theo đúng yêu cầu đề bài.")

    add_technique(doc, "3. Event-Driven Input (Lập trình hướng sự kiện)",
        "Là cơ chế 'lắng nghe' tương tác. Máy tính sẽ chờ đợi hành động cụ thể xảy ra từ phía người chơi.",
        "Hệ thống ghi nhận sự kiện Click chuột hoặc Chạm màn hình, lập tức kích hoạt mã lệnh bắn đạn.",
        "Đáp ứng yêu cầu 'Đối tượng C được bắn ra khi chạm màn hình', biến trò chơi thành một ứng dụng tương tác thực sự.")

    add_technique(doc, "4. Boundary Wrapping & RNG (Bọc biên & Tạo số ngẫu nhiên)",
        "Boundary Wrapping là kỹ thuật phát hiện vật thể đi ra ngoài mép màn hình để đưa nó quay lại. RNG là thuật toán tạo số ngẫu nhiên.",
        "Nếu Thiên thạch bay ra ngoài mép trái, game 'dịch chuyển' nó sang mép phải, đồng thời dùng RNG chọn tọa độ độ cao (Y) ngẫu nhiên mới.",
        "Thỏa mãn yêu cầu khó nhất của đề bài: 'B chạm biên sẽ xuất hiện lại ở biên đối diện với vị trí ngẫu nhiên', giúp game diễn ra vô tận.")

    add_technique(doc, "5. Object Instantiation (Nhân bản vật thể)",
        "Là kỹ thuật lấy một vật mẫu (Template/Prefab) có sẵn để copy ra thành nhiều bản sao trong lúc đang chơi.",
        "Khi click chuột, game lấy bản thiết kế Tia laser, đúc ra một bản sao (Clone) và đặt vào nòng súng của Phi thuyền.",
        "Cho phép tạo ra hàng loạt đạn (C) mỗi lần click mà không phải chuẩn bị trước hàng ngàn viên đạn trên màn hình.")

    # PHẦN 2
    doc.add_heading('PHẦN 2: CÁC KỸ THUẬT TỐI ƯU VÀ KIẾN TRÚC UNITY (Nâng cao)', level=1)
    
    add_technique(doc, "6. Frame-rate Independent Movement (Chuyển động độc lập khung hình - Time.deltaTime)",
        "Là kỹ thuật đồng bộ hóa tốc độ game với thời gian thực tế, loại bỏ sự phụ thuộc vào cấu hình máy tính.",
        "Bình thường, máy tính mạnh xử lý nhiều khung hình hơn máy yếu, khiến game chạy nhanh như tua nhanh. Bằng cách nhân tốc độ bay với 'thời gian hoàn thành 1 khung hình' (Time.deltaTime), vận tốc sẽ luôn được tính chuẩn xác theo đơn vị mét/giây.",
        "Đảm bảo Phi thuyền, Thiên thạch và Laser luôn di chuyển với một tốc độ ổn định, công bằng trên mọi thiết bị chấm điểm của giảng viên.")

    add_technique(doc, "7. Memory Management & Garbage Collection (Quản lý và Dọn dẹp bộ nhớ)",
        "Là thuật toán tự động dọn dẹp các vật thể không còn giá trị sử dụng để tránh đầy RAM.",
        "Khi Tia laser bay vượt ra ngoài tầm nhìn của Camera, hệ thống sẽ gọi hàm Destroy() để xé bỏ dữ liệu của nó khỏi bộ nhớ máy tính.",
        "Ngăn chặn lỗi tràn bộ nhớ (Memory Leak). Nếu không có kỹ thuật này, máy tính sẽ bị giật lag và đứng máy chỉ sau 1 phút bắn súng liên tục.")

    add_technique(doc, "8. Component-Based Architecture (Kiến trúc hướng thành phần)",
        "Là cách tổ chức mã lệnh đặc trưng của các nền tảng game hiện đại, hoạt động như việc lắp ráp khối Lego.",
        "Thay vì viết một đoạn code khổng lồ, trò chơi chia nhỏ thành các thành phần: SpriteRenderer (phụ trách vẽ hình ảnh), GameManager (phụ trách luật chơi).",
        "Giúp phân tách hoàn toàn giữa Logic và Hiển thị. Nhờ đó, nhóm có thể dễ dàng thay thế hình vuông thành hình Phi thuyền, Thiên thạch mà logic bay không hề bị hỏng.")

    add_technique(doc, "9. Orthographic Projection (Phép chiếu trực giao 2D)",
        "Là kỹ thuật xử lý camera loại bỏ quy luật xa gần (Perspective - vật ở xa trông nhỏ hơn).",
        "Camera được ép phẳng không gian thành môi trường 2D tuyệt đối. Mọi vật thể dù nằm ở độ sâu nào cũng giữ nguyên kích thước hiển thị.",
        "Đảm bảo đúng yêu cầu bài tập là môi trường chơi game 2D, Tàu (A) và Thiên thạch (B) luôn có tỷ lệ hiển thị kích thước bằng nhau một cách tuyệt đối trên màn hình phẳng.")

    add_technique(doc, "10. Editor Scripting & Automation (Tự động hóa công cụ Editor)",
        "Là kỹ thuật viết code để tự tạo ra các tính năng, công cụ bổ trợ ngay bên trong phần mềm lập trình (Unity).",
        "Thay vì phải tự tay kéo thả, cấu hình camera, xóa file rác, hệ thống có một đoạn mã riêng tạo ra nút bấm '1-Click Setup BT Game' trên thanh Menu. Nhấp vào đó, mọi thứ tự động được lắp ráp.",
        "Tăng tính chuyên nghiệp của project. Giảm thiểu rủi ro sai sót thao tác bằng tay khi báo cáo trước lớp. Thể hiện tư duy làm Tool (Công cụ) rất được đánh giá cao trong lập trình.")

    add_technique(doc, "11. Continuous Input Handling & Clamping (Điều khiển phím liên tục & Giới hạn biên)",
        "Là kỹ thuật đọc trạng thái nhấn giữ phím theo thời gian thực và dùng hàm toán học để khóa vật thể trong phạm vi cho phép.",
        "Mỗi khung hình, game kiểm tra phím W (di chuyển lên) hoặc S (di chuyển xuống). Hàm Mathf.Clamp() được áp dụng để đảm bảo tàu A không bao giờ bay vượt quá mép trên hoặc mép dưới của camera.",
        "Cung cấp cho người chơi quyền kiểm soát trực tiếp đối tượng A thay vì chỉ chạy tự động, biến game thành trải nghiệm điều khiển phi thuyền thực thụ.")

    add_technique(doc, "12. Mathematical Collision Detection & Game States (Phát hiện va chạm toán học & Trạng thái trò chơi)",
        "Là kỹ thuật tính toán khoảng cách hình học giữa các tâm vật thể (Euclidean Distance) để nhận biết va chạm mà không cần đến bộ máy vật lý (Physics Engine) cồng kềnh.",
        "Game liên tục so sánh khoảng cách: Nếu đạn C trúng thiên thạch B -> hủy C, tạo hiệu ứng nổ, cộng điểm và Respawn B ở mép phải. Nếu B đâm trúng tàu A -> kích hoạt trạng thái Game Over, dừng màn chơi và hiện bảng điểm kèm phím R để Chơi lại.",
        "Tạo nên vòng lặp gameplay hoàn chỉnh (Bắn trúng -> Thưởng điểm -> Bị đâm -> Game Over -> Chơi lại), thỏa mãn yêu cầu tương tác giữa các thực thể trong game.")

    add_technique(doc, "13. UV Texture Scrolling & Infinite Background (Cuộn nền vô tận qua UV Offset)",
        "Là kỹ thuật đồ họa tối ưu đỉnh cao: Thay vì di chuyển vị trí thật của vật thể trong không gian 3D/2D, ta chỉ trượt tọa độ ánh xạ ảnh (UV coordinates) trên bề mặt của vật thể phẳng (Quad).",
        "Kết hợp chế độ Wrap Mode = Repeat của Texture và lệnh cập nhật liên tục mainTextureOffset += (Speed * dt, 0) trong Update. Ảnh nền vũ trụ liên tục trôi từ phải sang trái mà không cần tạo mới hay di chuyển GameObject.",
        "Tạo cảm giác phi thuyền đang phóng với vận tốc siêu thanh vào vũ trụ sâu thẳm một cách mượt mà tuyệt đối, không tốn RAM và không gây giật lag (Zero Garbage Collection).")

    add_technique(doc, "14. Game State Machine & Title Screen Architecture (Kiến trúc Máy Trạng Thái & Màn hình Chờ)",
        "Là mô hình thiết kế quản lý vòng đời trò chơi thông qua các trạng thái hữu hạn (Finite State Machine - FSM): Menu chờ (StartMenu) -> Đang chơi (Playing) -> Thua cuộc (GameOver).",
        "Mỗi trạng thái đóng băng hoặc kích hoạt logic riêng: Ở Menu thì nền sao cuộn chậm, tàu chờ lệnh và hiện bảng hướng dẫn; khi bấm Bắt đầu thì mở khóa toàn bộ điều khiển và chuyển động; khi GameOver thì dừng game và cho chọn Chơi lại hoặc về Menu.",
        "Đem lại trải nghiệm người dùng hoàn chỉnh như một tựa game thương mại, giúp người chơi nắm rõ luật trước khi bắt đầu và không bị bỡ ngỡ khi vừa mở ứng dụng.")

    add_technique(doc, "15. Modern uGUI Canvas & Vector Font Scaler (Hệ Thống Canvas uGUI & Chống Vỡ Font)",
        "Là kỹ thuật thiết kế giao diện hiện đại của Unity: Chuyển đổi toàn bộ từ hệ thống vẽ điểm ảnh IMGUI OnGUI cũ sang hệ thống Canvas uGUI phân lớp chuyên nghiệp với CanvasScaler (Reference Resolution 1920x1080) và font Segoe UI vector.",
        "CanvasScaler tự động co giãn và tính toán lại kích cỡ các phần tử theo tỷ lệ màn hình thực tế. Ký tự tiếng Việt được dựng trực tiếp từ vector TrueType ở độ phân giải sắc nét nhất thay vì bị kéo dãn bitmap. Các bảng thông báo và nút bấm ứng dụng kỹ thuật 9-Slice bo góc tròn cùng hiệu ứng đổ bóng Shadow/Outline.",
        "Giải quyết triệt để vấn đề chữ hiển thị bị mờ, nhòe, vỡ hạt hoặc rách sọc ngang (scanline tearing) khi phóng to Game view hoặc chạy trên màn hình độ phân giải cao / Windows DPI scaling (125%, 150%). Đảm bảo UI luôn đạt chất lượng thẩm mỹ cao nhất cho buổi báo cáo.")

    add_technique(doc, "16. Hybrid Mobile Touch & Virtual Gamepad Architecture (Kiến Trúc Điều Khiển Cảm Ứng Di Động Đa Cơ Chế)",
        "Là hệ thống xử lý đầu vào (Input Handling) đa nền tảng kết hợp đồng thời giữa: Vuốt chạm trực tiếp (Touch & Drag), Cụm nút bấm ảo (Virtual Hold Buttons) và Bàn phím/Chuột máy tính truyền thống.",
        "Sử dụng các Interface IPointerDownHandler và IPointerUpHandler để tạo nút ảo giữ ngón tay di chuyển liên tục, kết hợp bộ lọc EventSystem.current.IsPointerOverGameObject để phân tách rõ ràng giữa việc bấm nút UI và việc vuốt màn hình. Tọa độ chạm được chuyển đổi sang World Space qua ScreenToWorldPoint để phi thuyền bám theo mượt mà.",
        "Đem lại khả năng tương thích 100% khi đóng gói và xuất sang Android Studio để chạy trên điện thoại hoặc máy ảo Android. Người chơi có thể tự do chọn cách vuốt ngón tay hoặc bấm nút ảo 2 bên như tay cầm chơi game.")
    
    file_path = r"c:\Users\ADMIN\Desktop\BT-game\Tai_Lieu_Ky_Thuat_Game_V2_Full.docx"
    try:
        doc.save(file_path)
        print(f"File updated successfully at {file_path}")
    except PermissionError:
        alt_path = r"c:\Users\ADMIN\Desktop\BT-game\Tai_Lieu_Ky_Thuat_Game_V3_Android.docx"
        doc.save(alt_path)
        print(f"File V2 dang duoc mo trong Word. Da luu thanh ban moi tai: {alt_path}")

if __name__ == "__main__":
    create_document()
