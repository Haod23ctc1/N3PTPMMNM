-- 2026-10-23: Bo sung mo ta chi tiet cho toan bo san pham (112 san pham)
-- Lenh chay: sqlcmd -S . -E -d DBNoiThat -f 65001 -i SQL\2026-10-23_mo_ta_san_pham_chi_tiet.sql
-- Ghi chu: bo qua ProductId = 123 (ban ghi thu nghiem khong co danh muc, khong hien tren web)

-- ========== PHONG AN ==========
UPDATE Product SET Description = N'Bộ bàn ăn gỗ tự nhiên hoàn thiện sơn PU bóng nhẹ, mặt bàn chống trầy xước và dễ lau chùi. Đi kèm bộ ghế đệm êm ái, vân gỗ tự nhiên khiến mỗi bữa cơm gia đình thêm phần ấm cúng.' WHERE ProductId = 1;
UPDATE Product SET Description = N'Bàn ăn cùng bộ ghế đồng bộ, khung gỗ chắc chắn chịu lực tốt, chân bàn được xử lý chống cong vênh. Thiết kế trung tính, dễ phối với nhiều kiểu không gian bếp: hiện đại, tân cổ điển hay tối giản.' WHERE ProductId = 2;
UPDATE Product SET Description = N'Bộ ghế ăn bọc nệm mềm mại, khung gỗ tự nhiên sơn màu trầm sang trọng. Đường viền chạm khắc tinh tế mang hơi hướng tân cổ điển, giúp phòng ăn của bạn thêm quý phái.' WHERE ProductId = 3;
UPDATE Product SET Description = N'Bộ bàn ăn hiện đại với mặt bàn chữ nhật tiện cho mọi loại món ăn. Bề mặt được phủ lớp chống ố vàng, giữ được độ sáng đẹp dù sử dụng suốt nhiều năm.' WHERE ProductId = 4;
UPDATE Product SET Description = N'Bàn ăn gỗ dày dặn, đường nét vuông vắn chuẩn mực. Bốn chân đặt vững chãi trên mọi loại nền nhà, ghế có tựa lưng ôm trọn giúp ngồi lâu vẫn không mỏi.' WHERE ProductId = 5;
UPDATE Product SET Description = N'Bộ bàn ăn thiết kế tối giản nhưng không đơn điệu, tập trung vào công năng và độ bền. Màu gỗ tự nhiên dễ phối cùng rèm cửa, đèn treo và cả tông màu tường của phòng ăn.' WHERE ProductId = 6;
UPDATE Product SET Description = N'Bàn ăn với mặt bàn rộng rãi, thoải mái bày đầy nồi bát cho những bữa tiệc gia đình. Gỗ được xử lý chống mối mọt cẩn thận, cam kết bền đẹp theo thời gian.' WHERE ProductId = 7;
UPDATE Product SET Description = N'Bộ bàn ăn hoàn thiện thủ công tỉ mỉ theo kinh nghiệm làng mộc truyền thống. Độ cao ghế cân đối với mặt bàn để mọi thành viên trong nhà đều ngồi thoải mái nhất.' WHERE ProductId = 8;
UPDATE Product SET Description = N'Bàn ăn chạm hoa văn ở bốn chân tạo điểm nhấn đậm chất Á Đông. Mặt phủ lớp chống thấm nước tốt, thoải mái đặt cả bát canh nóng lên mà không lo hư bề mặt.' WHERE ProductId = 9;
UPDATE Product SET Description = N'Bộ bàn ăn kích thước vừa vặn cho căn bếp phố, tiết kiệm diện tích nhưng vẫn đầy đủ công năng. Tông màu gỗ trầm ấm, dễ kết hợp đèn vàng cùng tường màu sáng.' WHERE ProductId = 10;
UPDATE Product SET Description = N'Bàn ăn phom dáng tân cổ điển với chân có điểm cong nhẹ cùng vân gỗ chạy rõ. Ghế bọc da tổng hợp bền màu, dễ lau sạch khi lỡ làm đổ nước hoặc dầu mỡ.' WHERE ProductId = 11;
UPDATE Product SET Description = N'Bộ bàn ăn ưu tiên sự bền bỉ: gỗ dày, trọng lượng thật, hoàn thiện kỹ từng mối ghép. Các cạnh được bo tròn an toàn, phù hợp gia đình có trẻ nhỏ hiếu động.' WHERE ProductId = 12;
UPDATE Product SET Description = N'Bàn ăn cỡ lớn cho gia đình đông thành viên hoặc nhà hay tổ chức tiệc. Chất gỗ được xử lý kỹ trước khi chế tác, hạn chế tối đa co ngót hở khe theo thời gian dùng.' WHERE ProductId = 13;
UPDATE Product SET Description = N'Bộ bàn ăn nhẹ nhàng hiện đại, ưu tiên sự gọn gàng dễ dọn dẹp sau bữa ăn. Mặt bàn phủ bề mặt lâu bền màu, ghế đệm dễ thay mới khi muốn làm mới không gian.' WHERE ProductId = 14;

-- ========== PHONG KHACH ==========
UPDATE Product SET Description = N'Bộ đồ phòng khách gỗ tự nhiên chạm khắc tinh xảo, phủ sơn PU bóng nhẹ nhàng. Sofa khung gỗ dày dặn với đệm bọc êm ái, tạo không gian tiếp khách sang trọng và thoải mái.' WHERE ProductId = 15;
UPDATE Product SET Description = N'Nội thất phòng khách đồng bộ từ sofa, bàn trà tiện kệ trang trí. Màu gỗ trầm cùng đường vân rõ nét giúp căn phòng có chiều sâu và mang cá tính riêng của gia chủ.' WHERE ProductId = 16;
UPDATE Product SET Description = N'Bộ phòng khách gây ấn tượng với các họa tiết hoa văn chạm truyền thống cùng tông sơn trầm quý phái. Dưới ánh đèn vàng, từng đường nét trên gỗ càng được tôn rõ vẻ đẹp.' WHERE ProductId = 17;
UPDATE Product SET Description = N'Bộ đồ phòng khách tối giản hiện đại, ưu tiên công năng và sự thoải mái mỗi ngày. Khung gỗ vững chãi cùng lớp đệm dày giúp ngồi lâu không mỏi, dễ vệ sinh sạch sẽ.' WHERE ProductId = 18;
UPDATE Product SET Description = N'Nội thất phòng khách hợp nhà ống phố: kích thước gọn gàng nhưng chẳng thiếu công năng. Bàn trà có ngăn kéo tiện chứa điều khiển, ấm trà và những vật dụng hay dùng.' WHERE ProductId = 19;
UPDATE Product SET Description = N'Sofa kiểu chỗ ngồi thấp lấy cảm hứng Á Đông thanh lịch, phù hợp nhà ống hay căn hộ cần tối ưu diện tích. Vẫn giữ trọn nét sang trọng trong từng đường chạm khắc.' WHERE ProductId = 20;
UPDATE Product SET Description = N'Bộ phòng khách tân cổ điển mềm mại với những đường cong thanh thoát. Chất liệu gỗ tự nhiên bền chắc, đệm bọc vải nỉ cao cấp tháo rời thuận tiện giặt sạch.' WHERE ProductId = 21;
UPDATE Product SET Description = N'Bộ phòng khách hiện đại tối giản với đường nét thẳng gọn gàng. Màu gỗ trung tính dễ phối rèm cửa và thảm theo mùa hoặc theo ý thích của gia chủ.' WHERE ProductId = 22;
UPDATE Product SET Description = N'Bộ phòng khách trau chuốt từng chi tiết: đường kim mạch đệm cài cẩn thận, bề mặt gỗ đánh bóng màu đồng đều. Hợp cả tiếp đón khách quý và đối tác ghé thăm nhà.' WHERE ProductId = 23;
UPDATE Product SET Description = N'Bộ phòng khách đa năng gồm sofa, bàn trà và kệ trang trí theo đồng bộ một tông gỗ. Kết cấu vững chắc, chất liệu dễ vệ sinh, giúp bạn tiết kiệm thời gian chăm sóc.' WHERE ProductId = 24;

-- ========== PHONG THO ==========
UPDATE Product SET Description = N'Bộ đồ thờ chạm khắc thủ công theo đề tài truyền thống mang ý nghĩa sung túc, bình an. Gỗ tự nhiên nặng tay, lớp sơn phủ đều màu, tôn nét trang nghiêm cho gian thờ nhà bạn.' WHERE ProductId = 25;
UPDATE Product SET Description = N'Bàn thờ chế tác từ gỗ tự nhiên dày dặn, đường vân chạy thẳng mượt, chân bàn vững chãi. Hoạ tiết được chạm thủ công tỉ mỉ, chuẩn mực tục lệ của không gian thờ cúng.' WHERE ProductId = 26;
UPDATE Product SET Description = N'Bộ đồ thờ đồng bộ từ bàn, án trước đến hoành phi, câu đối. Đường chạm sâu và sắc nét, mang lại vẻ trang nghiêm toàn vẹn cho gian thờ.' WHERE ProductId = 27;
UPDATE Product SET Description = N'Bàn thờ gỗ tự nhiên mặt rộng, chân đế chắc chắn, thuận tiện bày biện bát hương, mâm quả. Bề mặt sơn phủ chọn lọc, dễ lau chùi giữ gìn nét trang nghiêm quanh năm.' WHERE ProductId = 28;
UPDATE Product SET Description = N'Bộ thờ gồm bàn thờ cùng án ngồi và hoành phi, câu đối chạm đồng bộ. Nét chạm đều tay, hoa văn đối xứng chuẩn vị, phù hợp nhà thờ truyền thống ở cả thành phố và nông thôn.' WHERE ProductId = 29;
UPDATE Product SET Description = N'Bàn thờ kiểu hiện đại pha nét chạm truyền thống nhẹ nhàng. Cấu trúc chắc chắn, chia ngăn hợp lý giúp sắp xếp mâm quả, đèn nến, lọ hoa hài hòa, cân đối.' WHERE ProductId = 30;
UPDATE Product SET Description = N'Bộ đồ thờ chạm đề tài tùng, cúc, trúc, mai – biểu trưng cho đức độ và sự trường tồn. Gỗ được tuyển chuẩn, hoàn thiện tay nên bề mặt mịn và bền màu theo năm tháng.' WHERE ProductId = 31;
UPDATE Product SET Description = N'Bàn thờ chân đế chạm hoa văn nổi, mặt bàn phẳng mịn, màu gỗ trầm sang. Ngăn dưới tiện chỗ để những đồ thờ cúng ít dùng hàng ngày.' WHERE ProductId = 32;
UPDATE Product SET Description = N'Án thờ được chạm khắc tay đồng bộ với bàn thờ, phủ lớp sơn đều màu. Giúp gian thờ thêm cân đối, trang trọng đúng nghĩa.' WHERE ProductId = 33;
UPDATE Product SET Description = N'Tủ thờ gỗ tự nhiên kiểu dáng đơn giản nhưng chuẩn mực. Ngăn tủ chia đều tiện đựng đồ lễ, ly trà và vật dụng thờ cúng một cách gọn gàng.' WHERE ProductId = 34;
UPDATE Product SET Description = N'Bộ đồ thờ ưu tiên chất gỗ chắc nặng, hoàn thiện bằng tay theo tay nghề nghệ nhân lâu năm. Hoa văn chạm mượt, đối xứng chuẩn, giữ gìn dễ dàng chỉ bằng khăn mềm.' WHERE ProductId = 35;
UPDATE Product SET Description = N'Bàn thờ thiết kế cân đối, kích thước đa dạng để chọn theo diện tích phòng. Gỗ tự nhiên hoàn thiện nhẹ nhàng, giữ màu bền đẹp trong mọi điều kiện của không gian trong nhà.' WHERE ProductId = 36;

-- ========== PHONG NGU ==========
UPDATE Product SET Description = N'Bộ đồ phòng ngủ gồm giường, tủ quần áo và bàn đầu giường theo cùng một tông gỗ. Đầu giường chạm hoa văn nhẹ nhàng, tạo cảm giác thư thái cho giấc ngủ mỗi đêm.' WHERE ProductId = 37;
UPDATE Product SET Description = N'Nội thất phòng ngủ thiết kế hiện đại, tối ưu công năng cho không gian nhỏ. Giường có không gian chứa đồ gọn gàng bên dưới, tủ cánh mở êm nhẹ nhàng.' WHERE ProductId = 38;
UPDATE Product SET Description = N'Bộ phòng ngủ tông màu gỗ sáng trung tính, dễ phối ga giường và rèm cửa theo mùa. Chất gỗ chịu lực tốt, khung giường không rung lắc dù dùng nhiều năm.' WHERE ProductId = 39;
UPDATE Product SET Description = N'Bộ đồ phòng ngủ phong cách tân cổ điển với các đường cong mềm mại. Chi tiết chạm khắc ở đầu giường tinh tế, giúp không gian riêng tư vừa sang vừa ấm áp.' WHERE ProductId = 40;
UPDATE Product SET Description = N'Giường ngủ gỗ tự nhiên với tấm tựa đầu cao rộng, thoải mái tựa lưng đọc sách trước giờ ngủ. Khung sườn chịu lực tốt, không rung lắc dù dùng liên tục nhiều năm.' WHERE ProductId = 41;
UPDATE Product SET Description = N'Bộ phòng ngủ gọn nhẹ dành cho phòng nhỏ: giường tiêu chuẩn cùng tủ hai buồng tiện chia đồ của hai vợ chồng. Mọi món đều đồng bộ tông gỗ, nhìn gọn gàng ngay từ cửa phòng.' WHERE ProductId = 42;
UPDATE Product SET Description = N'Bộ đồ phòng ngủ tông màu trầm ấm, giúp không gian nghỉ ngơi tĩnh lặng, dễ đi vào giấc ngủ. Tủ quần áo chia ngăn hợp lý, cánh mở êm và bền.' WHERE ProductId = 43;
UPDATE Product SET Description = N'Giường ngủ kiểu thấp gần sàn pha hơi hướng Á Đông thanh mảnh. Phù hợp người thích cảm giác mộc mạc, ổn định, ít lo lắc kêu khi trở mình giữa đêm.' WHERE ProductId = 44;
UPDATE Product SET Description = N'Bộ phòng ngủ đầy đủ giường, tủ và bàn đầu giường, mọi thứ đồng bộ một tông gỗ. Đường nét tinh gọn, dễ dọn dẹp, phù hợp cả phòng chính lẫn phòng cho con.' WHERE ProductId = 45;
UPDATE Product SET Description = N'Bộ đồ phòng ngủ tối giản kiểu Bắc Âu pha chất liệu gỗ tự nhiên. Màu sắc trung tính dễ kết hợp ga giường, đèn ngủ và thảm trên sàn theo gu riêng của bạn.' WHERE ProductId = 46;
UPDATE Product SET Description = N'Bộ phòng ngủ bền bỉ kiểu truyền thống, khung gỗ dày và các mối ghép vuông vắn. Đầu giường với họa tiết chạm nhẹ tạo điểm nhấn mà không rườm rà.' WHERE ProductId = 47;

-- ========== PHONG LAM VIEC ==========
UPDATE Product SET Description = N'Bộ nội thất phòng làm việc gồm bàn lớn, ghế tựa êm và kệ sách đồng bộ. Mặt bàn rộng thoải mái đặt máy tính, bàn phím và cả đống tài liệu làm việc hàng ngày.' WHERE ProductId = 48;
UPDATE Product SET Description = N'Bàn làm việc khung gỗ chắc chắn, mặt phẳng chịu lực tốt, gần như không rung khi gõ phím mạnh. Phù hợp cả làm việc tại nhà lẫn học bài lâu dài.' WHERE ProductId = 49;
UPDATE Product SET Description = N'Bộ bàn ghế làm việc thiết kế công thái học: ghế có độ cao cân đối với mặt bàn, tựa lưng ôm nhẹ cột sống nên ngồi lâu ít mỏi vai và lưng.' WHERE ProductId = 50;
UPDATE Product SET Description = N'Bộ nội thất văn phòng tại nhà tông gỗ trung tính, tạo cảm giác tập trung. Kệ sách nhiều tầng giúp sắp xếp tài liệu, sách vở ngăn nắp, trông gọn gàng ngay từ cái nhìn đầu tiên.' WHERE ProductId = 51;
UPDATE Product SET Description = N'Bàn làm việc có hộc kéo rộng chứa văn phòng phẩm, giúp mặt bàn luôn sạch sẽ gọn gàng. Các cạnh được bo mềm, an toàn khi có con nhỏ chạy lại gần.' WHERE ProductId = 52;
UPDATE Product SET Description = N'Bàn ghế làm việc kiểu gọn nhẹ dành cho góc học tập hay góc làm việc trong căn hộ chung cư. Chất gỗ bền, dễ tháo lắp khi chuyển nhà, vẫn giữ nguyên độ chắc chắn.' WHERE ProductId = 53;
UPDATE Product SET Description = N'Bộ làm việc đường nét thẳng hiện đại, mặt bàn phủ chống trầy giúp bền màu theo thời gian. Tông gỗ sáng nhẹ giúp góc làm việc trông thông thoáng, bớt ngột ngạt.' WHERE ProductId = 54;
UPDATE Product SET Description = N'Bàn làm việc mặt dài phù hợp đặt hai máy tính cạnh nhau để làm chung hoặc cho hai con học bài. Khung hỗ trợ ở giữa tăng độ cứng, mặt bàn không võng khi chịu lực nặng.' WHERE ProductId = 55;
UPDATE Product SET Description = N'Bộ nội thất làm việc chuẩn văn phòng: đầy đủ bàn, ghế và tủ phụ đồng bộ. Giúp góc làm việc tại nhà nghiêm túc, chuyên nghiệp như đang ngồi trong văn phòng.' WHERE ProductId = 56;
UPDATE Product SET Description = N'Bàn làm việc kiểu chữ L tận dụng tối đa góc phòng, vừa soạn máy tính vừa ký duyệt tài liệu trên cùng không gian. Mặt bàn chia vùng rõ ràng, tiện tổ chức dây cáp.' WHERE ProductId = 57;
UPDATE Product SET Description = N'Bàn làm việc có kệ trên tiện để sách, chậu cây nhỏ cùng đèn bàn. Tạo góc làm việc thân thiện, dễ nảy sinh cảm hứng mỗi sáng đến bàn.' WHERE ProductId = 58;

-- ========== BAN LAM VIEC ==========
UPDATE Product SET Description = N'Bàn làm việc gỗ tự nhiên thiết kế đơn giản bền bỉ, mặt bàn phẳng chịu lực tốt. Hộc bàn trượt êm chứa tài liệu, giúp bàn làm việc luôn có trật tự.' WHERE ProductId = 59;
UPDATE Product SET Description = N'Bàn làm việc hiện đại chân thẳng mạnh mẽ, mặt bàn phủ chống ố vàng. Thiết kế hợp cả học sinh, sinh viên lẫn người làm việc tại nhà trong thời gian dài.' WHERE ProductId = 61;
UPDATE Product SET Description = N'Bàn làm việc đi kèm ngăn kệ trên tiện để sách và đèn học. Chiều cao chuẩn giúp ngồi đúng tư thế, tránh mỏi cổ và vai khi học hoặc làm việc cả buổi.' WHERE ProductId = 63;
UPDATE Product SET Description = N'Bàn làm việc với đường nét ưu tiên công năng, góc đặt dây cáp gọn gàng. Chất liệu gỗ bền, khả năng chịu tải tốt, dùng ổn định trên mọi loại nền nhà.' WHERE ProductId = 65;

-- ========== BAN AN ==========
UPDATE Product SET Description = N'Bàn ăn gỗ tự nhiên thiết kế tròn đặc, tiện cho cả nhà quay mặt trò chuyện trong bữa ăn. Chất gỗ chắc, khi bày thêm món mới cũng không lo trầy xước mặt bàn.' WHERE ProductId = 62;
UPDATE Product SET Description = N'Bàn ăn dáng chữ nhật gọn hợp không gian bếp hẹp. Các mối ghép xử lý chắc chắn, đứng chân ổn định trên nền không đều, không rung khi ngồi dựa.' WHERE ProductId = 64;
UPDATE Product SET Description = N'Bàn ăn cỡ lớn hoàn thiện kỹ lưỡng, phù hợp nhà đông người hay mở tiệc chiêu đãi. Đường vân gỗ nổi bật, càng dùng lâu màu càng đậm chất quý.' WHERE ProductId = 66;

-- ========== KE ==========
UPDATE Product SET Description = N'Kệ gỗ đa năng kiểu chữ nhật, chia tầng hợp lý tiện bày sách, chậu cây hay đồ trang trí. Khung đứng khoẻ, mỗi tầng chịu lực tốt mà vẫn giữ vẻ mảnh mai.' WHERE ProductId = 67;
UPDATE Product SET Description = N'Kệ trưng bày với hoa văn chạm nhẹ ở rìa tầng, vừa để đồ vừa tự nó đã là một món trang trí. Màu gỗ trầm hợp đặt trong phòng khách hoặc hành lang.' WHERE ProductId = 68;
UPDATE Product SET Description = N'Kệ gỗ kiểu đơn giản dễ lắp đặt trong mọi không gian: phòng khách, phòng làm việc hay phòng ngủ. Chất liệu chống mối mọt, dễ lau chùi, dùng lâu không bong tróc.' WHERE ProductId = 69;
UPDATE Product SET Description = N'Kệ khung to hoàn thiện cao cấp, tầng gỗ dày chịu lực lớn, cất cả máy in hay chồng sách dày đều vững vàng. Thiết kế sang trọng, hợp làm điểm nhấn cho phòng làm việc.' WHERE ProductId = 70;
UPDATE Product SET Description = N'Kệ gỗ dáng thẳng đứng tiết kiệm diện tích sàn nhưng vẫn có nhiều tầng chứa. Phù hợp đặt cạnh bàn làm việc hoặc giữa phòng như một vách ngăn trang trí nhẹ nhàng.' WHERE ProductId = 71;
UPDATE Product SET Description = N'Kệ với chiều cao vừa vặn, tiện dựng phía trên bàn hoặc bên cửa sổ. Giúp sắp xếp vật hay dùng đúng tầm tay mà không chiếm chỗ trên mặt bàn.' WHERE ProductId = 72;
UPDATE Product SET Description = N'Kệ gỗ thiết kế ô trưng bày tiện đặt tiểu cảnh, tượng phong thủy hay bộ sưu tập của bạn. Đường nét chắc chắn, trông quý phái mà không rườm rà.' WHERE ProductId = 73;

-- ========== NEM ==========
UPDATE Product SET Description = N'Nệm mút đàn hồi vừa phải, ôm người dễ chịu, hạn chế đau lưng khi ngủ. Vỏ bọc vải thấm hút tốt, có khóa kéo tháo giặt tiện vệ sinh định kỳ.' WHERE ProductId = 74;
UPDATE Product SET Description = N'Nệm lõi mút mềm phẳng đều, hỗ trợ cột sống giữ tư thế tự nhiên khi ngủ. Bề mặt vải thoáng khí, nằm mát dễ chịu cả bốn mùa.' WHERE ProductId = 75;
UPDATE Product SET Description = N'Nệm cao cấp với hệ lò xo túi độc lập, mỗi lò xo ôm riêng từng vùng cơ thể. Khi trở mình, người bên cạnh gần như không hề cảm nhận bị ảnh hưởng.' WHERE ProductId = 76;
UPDATE Product SET Description = N'Nệm cao cấp với lớp mút memory ôm theo đường cong cơ thể, giảm áp lực vai và hông. Dù nằm ngửa hay nghiêng, lưng vẫn được đỡ trọn vẹn suốt giấc ngủ.' WHERE ProductId = 77;
UPDATE Product SET Description = N'Nệm độ êm chuẩn khách sạn, chế tạo nhiều lớp: lõi lò xo bên trong cùng lớp mút êm phía trên. Tận hưởng cảm giác nằm nghỉ sang trọng ngay tại ngôi nhà của mình.' WHERE ProductId = 78;

-- ========== TU (Phong cach Korea) ==========
UPDATE Product SET Description = N'Tủ thiết kế theo phong cách Hàn Quốc trẻ trung, tông màu gỗ sáng ấm. Ngăn tủ chia khoa học, cánh mở êm, phù hợp phòng ngủ hay phòng khách căn hộ hiện đại.' WHERE ProductId = 79;
UPDATE Product SET Description = N'Tủ cỡ gọn tiện đặt phòng nhỏ, cạnh giường hoặc lối vào ban công. Dù gọn nhưng khung tủ chắc chắn, dễ di chuyển khi dọn phòng hay sắp xếp lại nhà.' WHERE ProductId = 80;
UPDATE Product SET Description = N'Tủ hai buồng rộng thuận tiện treo áo dài, xếp khăn vải và cất đồ theo ngăn rõ ràng. Cánh tủ mở êm, bản lề chắc, dùng bền qua nhiều năm.' WHERE ProductId = 81;
UPDATE Product SET Description = N'Tủ phong cách Hàn Quốc với các đường bo mềm và tay nắm tối giản. Bề mặt sơn đều màu, dễ phối cùng tủ giày, kệ TV theo đúng phong cách trong nhà.' WHERE ProductId = 82;
UPDATE Product SET Description = N'Tủ quy mô lớn hoàn thiện cao cấp, không gian chứa đồ dồi dào cho cả gia đình. Thiết kế chuẩn Hàn Quốc trẻ trung nhưng vẫn giữ nét vững chãi, bền bỉ.' WHERE ProductId = 83;
UPDATE Product SET Description = N'Tủ đa năng nhiều tầng, vừa để sách vở, vừa cất đồ gia dụng hay đặt chậu cây trang trí. Kích thước nhỏ gọn, góc nào trong nhà cũng có thể đặt vừa.' WHERE ProductId = 84;
UPDATE Product SET Description = N'Tủ có nhiều ngăn cấp thuận tiện cất quần áo, sách vở và đồ dùng học của các con theo từng nhóm riêng. Chất gỗ an toàn, cạnh bo tròn, không lo trẻ đụng phải xây xát.' WHERE ProductId = 85;
UPDATE Product SET Description = N'Tủ cao cấp kiểu Hàn Quốc với vân gỗ nổi bật, hoàn thiện sơn bóng nhẹ. Thiết kế cân đối sang trọng, là điểm nhấn hoàn hảo cho phòng ngủ chính.' WHERE ProductId = 86;

-- ========== TU CA NHAN (Phong cach Havana) ==========
UPDATE Product SET Description = N'Tủ cá nhân theo phong cách Havana khỏe khoắn, đậm chất cá tính riêng biệt. Khung gỗ chắc, cánh đóng mở mượt, hợp làm tủ đồ cá nhân trong phòng ngủ.' WHERE ProductId = 87;
UPDATE Product SET Description = N'Tủ cá nhân cỡ gọn giúp cất riêng đồ dùng cá nhân: giấy tờ, phụ kiện và trang phục. Dễ đặt cạnh bàn làm việc hoặc đầu giường ngay cả trong phòng nhỏ.' WHERE ProductId = 88;
UPDATE Product SET Description = N'Tủ cá nhân có ngăn khóa tiện cất giấy tờ, laptop hay đồ nhỏ quý giá. Cấu trúc chắc chắn, trọng lượng ổn định nên khó bị đẩy dịch chuyển.' WHERE ProductId = 89;
UPDATE Product SET Description = N'Tủ cá nhân giá hợp lý nhưng hoàn thiện cẩn thận từ chất gỗ bền. Lựa chọn tối ưu cho phòng trọ, phòng thuê hay văn phòng nhỏ mới dựng.' WHERE ProductId = 90;
UPDATE Product SET Description = N'Tủ cá nhân chia ngăn nhiều tầng giúp sắp xếp đồ theo chủ đề rõ ràng, cần gì lấy ngay ra. Kiểu dáng tối giản, dễ phối với bất cứ cách bày phòng nào.' WHERE ProductId = 91;
UPDATE Product SET Description = N'Tủ cá nhân nhỏ có tay nắm kim loại chắc chắn, đóng mở nhanh gọn. Chất gỗ xử lý kỹ, không cong vênh ngay cả khi đặt ở chỗ có nắng nhiều.' WHERE ProductId = 92;
UPDATE Product SET Description = N'Tủ cá nhân với ngăn kéo sâu tiện cất đồ cồng kềnh: áo khoác, chăn màn mỏng hay hộp giấy tờ lớn. Ray trượt êm, đóng mở nhiều lần vẫn không bị kẹt.' WHERE ProductId = 93;
UPDATE Product SET Description = N'Tủ cá nhân tông màu gỗ trầm lịch lãm, hợp setup phòng làm việc tại nhà. Không gian cất đồ chia tầng hợp lý nên đồ đạc luôn gọn gàng đúng vị trí.' WHERE ProductId = 94;

-- ========== BAN TRANG DIEM ==========
UPDATE Product SET Description = N'Bàn trang điểm cao cấp với gương soi lớn, ngăn kéo ngang tiện cất mỹ phẩm. Thiết kế sang trọng tinh tế, là góc chăm sóc sắc đẹp lý tưởng cho chị em.' WHERE ProductId = 112;
UPDATE Product SET Description = N'Bàn trang điểm duyên dáng với gương soi rộng, ánh sáng phản chiếu tự nhiên. Ngăn kéo chia gọn mỹ phẩm, lược chải và trang sức theo từng nhóm riêng.' WHERE ProductId = 113;
UPDATE Product SET Description = N'Bàn trang điểm nhỏ gọn hợp phòng trọ hay phòng ngủ chật hẹp. Vẫn đầy đủ gương soi cùng ngăn kéo để bài trí nguyên bộ mỹ phẩm hằng ngày.' WHERE ProductId = 114;

-- ========== TU HO SO ==========
UPDATE Product SET Description = N'Tủ hồ sơ chất lượng văn phòng cao cấp, nhiều tầng kéo sâu chịu lực tốt. Phù hợp lưu trữ hợp đồng, sổ sách quan trọng của công ty trong nhiều năm.' WHERE ProductId = 223;
UPDATE Product SET Description = N'Tủ hồ sơ gọn nhẹ phù hợp văn phòng nhỏ hay góc làm việc tại nhà. Ngăn kéo chia rành mạch, dán nhãn dễ, cần tìm tài liệu chỉ mất vài giây.' WHERE ProductId = 224;
UPDATE Product SET Description = N'Tủ hồ sơ có chốt khóa an toàn, cất giữ tài liệu cần bảo mật hoàn toàn yên tâm. Cấu trúc gỗ vững chắc, các tầng xếp giấy đứng thẳng không bị võng dù chứa nhiều.' WHERE ProductId = 225;
UPDATE Product SET Description = N'Tủ hồ sơ dựng đứng tiết kiệm diện tích sàn, tận dụng tốt chiều cao của phòng. Mỗi tầng rộng rãi, sắp xếp theo dự án hay theo năm đều thuận tiện.' WHERE ProductId = 226;
UPDATE Product SET Description = N'Tủ hồ sơ có mặt trên bằng phẳng tiện dùng làm kệ để máy in hoặc khay thư. Mọi nhu cầu văn phòng thường ngày đều nằm gọn trong một góc duy nhất.' WHERE ProductId = 227;
UPDATE Product SET Description = N'Tủ hồ sơ vững chãi kiểu truyền thống với khung gỗ dày. Dù chứa đầy giấy tờ cả năm, tầng kéo vẫn đóng mở êm không hề bị kẹt.' WHERE ProductId = 228;
UPDATE Product SET Description = N'Tủ hồ sơ cao cấp hoàn thiện kỹ từ chất liệu đến từng ray trượt đều đạt chuẩn. Giúp văn phòng trông chuyên nghiệp, đúng hình ảnh doanh nghiệp muốn gửi đến khách.' WHERE ProductId = 229;

-- ========== TU GIAY ==========
UPDATE Product SET Description = N'Tủ giấy dung tích lớn phục vụ văn phòng bận rộn: file hồ sơ, giấy A4 và bìa đựng đều xếp gọn. Chất liệu cao cấp bền màu, dùng ổn định hàng chục năm.' WHERE ProductId = 230;
UPDATE Product SET Description = N'Tủ giấy mini hợp góc làm việc nhỏ, đặt trên bàn hoặc dưới gầm đều tiện dụng. Giúp giấy tờ hằng ngày ngăn nắp, hết cảnh giấy vương vãi.' WHERE ProductId = 231;
UPDATE Product SET Description = N'Tủ giấy với nhiều ngăn tiện phân loại hồ sơ theo hạng mục. Ghi nhãn từng ngăn giúp tìm đúng giấy tờ giữa bộn bề công việc chỉ trong vài giây.' WHERE ProductId = 232;
UPDATE Product SET Description = N'Tủ giấy thiết kế chuẩn văn phòng truyền thống, chắc chắn theo thời gian. Các ray kéo bền, nếu hao mòn cũng dễ thay thế mà không phải tháo cả khung.' WHERE ProductId = 234;
UPDATE Product SET Description = N'Tủ giấy nhẹ nhàng dễ di chuyển khi dọn phòng hay đổi chỗ làm việc. Bề mặt sơn chống trầy, giữ nét trong sáng cho không gian làm việc.' WHERE ProductId = 235;

-- ========== TU QUAN AO ==========
UPDATE Product SET Description = N'Tủ quần áo nhỏ gọn phù hợp phòng cho thuê hay phòng đơn của người trẻ. Đựng vừa đủ trang phục hàng ngày, đóng mở nhẹ nhàng, dễ di chuyển khi cần.' WHERE ProductId = 236;
UPDATE Product SET Description = N'Tủ hai cánh tiện chia đồ theo mùa rõ ràng: áo phông một bên, áo phao một bên. Ngăn khung treo giữ váy áo không nhàu, ngăn dưới xếp đồ gọn gàng.' WHERE ProductId = 238;
UPDATE Product SET Description = N'Tủ quần áo mini siêu tiết kiệm cho phòng nhỏ nhất: hợp cất đồ mỏng, đồ theo mùa. Dễ lắp đặt, cất vào góc nào trong phòng cũng vừa vặn.' WHERE ProductId = 239;
UPDATE Product SET Description = N'Tủ quần áo cao cấp cỡ lớn, không gian treo và xếp đồ rộng rãi cho cả gia đình. Hoàn thiện hoàn hảo từ khung đến cánh, đóng mở êm suốt nhiều năm.' WHERE ProductId = 240;
UPDATE Product SET Description = N'Tủ quần áo mini kết cấu nhẹ, tiện cho người ở trọ hay hay chuyển nhà. Dù nhẹ, khung vẫn ổn định, treo váy áo không bị võng xuống.' WHERE ProductId = 241;

GO
