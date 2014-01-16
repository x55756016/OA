//从服务端的数据库中加载数据
function creat_rd_arry() {
    if (lucky.Ticket.length == 0) {
        var awardExisted = dbProxy.GetAwardObj(); //DBProxy.js
        loadData(lucky, awardExisted); //Lucky.js
    }
    
}

//开始滚动
function stop(level, number) {
     clearInterval(m);
    //获取得奖号
    var obj = lucky.GenerateAward(level, number);
    $("#roll_num").css("display", "none");
    $("#num_award").css("display", "block");
    if (level == 3) {
        if (obj[0] != null) {
            $("#num_award")[0].innerHTML = "<li>" + obj[0] + "</li>";
        }
        if (obj[2].length > 0) {
            //show_3Info(obj); //三等奖显示
            dbProxy.AddAwardMany(obj[2]);
        }
    }
    else {
        //if (level == 0) {
        //    $("#num_award")[0].innerHTML = "<li>" + obj.TicketNO + "</li>";
        //    //上传保存获奖号
        //    dbProxy.AddAward(obj[0].TicketNO, obj[0].Level, ""); //DBProxy.js
        //}
        if (obj.length > 0) { //要有数才行
            //插入获奖号
            var award_li_html = '';
            for (i = 0; i < number; i++) {
                award_li_html += "<li>" + obj[i].TicketNO + "</li>";
                if (i == obj.length - 1) break;
            }
            $("#num_award")[0].innerHTML = award_li_html;
            //上传保存获奖号
            dbProxy.AddAwardMany(obj);
            show_award(cur_lev, batch);
        }
    }
}
function start() {
    $("#roll_num").css("display", "block");
    $("#num_award").css("display", "none").empty();
    mov(cur_num); //Default.js
}

//用于显示三等奖的人数信息及中奖范围
var szcount = 0;
//var bjcount = 0;
var szThird_li = '', bjThird_li = '';
function show_3Info(obj) {
    var $sz = $(".sz_award"), $bj = $(".bj_award");
        szcount = obj[2].length;
        szThird_li += "</br><li>" + "人数：" + "</li>";
        szThird_li += "<li>" + szcount + "</li>";
    $sz[0].innerHTML = szThird_li;
    //$bj[0].innerHTML = bjThird_li;
}

//两边显示获奖号
function show_award(cur_lev, batch) {
    if (cur_lev == 1 || cur_lev == 2 || cur_lev == 4) {//一二等级才显示
        var sz_li = '', bj_li = '', $sz = $(".sz_award"), $bj = $(".bj_award");
        var awards_sz_all = lucky.GetAwardBy("", cur_lev); //获取该等奖的号码
        awards_sz_all.sort(); //排序
        var num = awards_sz_all.length;
        var total = num;
        if (cur_lev == 2 || cur_lev == 4) {//二等奖,因为只有一二等奖走这个方法
            total = 10;//二等奖20人，一边显示10人
            if (num < 10) {
                total = num;
            }
        }
        else {//一等奖
            total = 3; //一等奖6人，一边显示3人
            if (num < 3) {
                total = num;
            }
        }
        for (i = 0; i < total; i++) {
            sz_li += "<li>" + awards_sz_all[i] + "</li>";
        }
        for (i = total; i < num; i++) {
            bj_li += "<li>" + awards_sz_all[i] + "</li>";
        }
        $sz[0].innerHTML = sz_li;
        $bj[0].innerHTML = bj_li;
    }
}

///S键产生一个中奖号码
function stopOne(loc, level) {
    // clearInterval(m);
    lucky.getArray();
    //获取得奖号
    var obj = lucky.generateOneAward(level); //Lucky.js
    $("#roll_num").css("display", "none");
    $("#num_award").css("display", "block");
    //插入获奖号
    var award_li_html = ''
    award_li_html += "<li>" + obj.TicketNO + "</li>";
    $("#num_award")[0].innerHTML = award_li_html;
    //上传保存获奖号
    dbProxy.AddAward(obj.TicketNO, obj.Level, "");
    show_award(cur_lev, batch);
}

function stop_loc(loc) {
    clearInterval(m);
    
    //获取得奖号
    if (loc == 'BJ') {
        var obj = lucky.GenerateAward0BJ(); //Lucky.js
    } else {
        var obj = lucky.GenerateAward0SZ();
    }
    //插入获奖号    
    $("#num_award").append("<li>" + obj.TicketNO + "</li>");

    //停止动画，显示得奖号
    $("#num_award").css("display", "block");
    $("#roll_num").css("display", "none");
    
    

    //上传获奖号保存至数组，以便抽奖结束后打印抽奖号码
    dbProxy.AddAward(obj.TicketNO, obj.Level, ""); //DBProxy.js
    
}

var all_array = [],all_array_length=0;

//生产序号（自动补零）,loc为SZ或者BJ，a为开始值，b为结束值，c为位数
function sn(loc, a, b, c) {
    while (a <= b) {
        t = a + "";
        while (t.length < c) { t = "0" + t; };
        //t = loc + t;
        a++;
        all_array.push(t);        
    }
}

var szCount = 0;
//var bjCount = 0;
//从服务端的数据库中加载票数
this.getTicketCount = function() {
    var ticketCount = dbProxy.getTicketCount(); //DBProxy.js
        //bjCount = ticketCount[0];
        szCount = ticketCount[1]
}
//生成深圳、北京序号
function all_sn() {
    this.getTicketCount();
    sn('', 1, szCount, 4);
    //sn('', 1, bjCount, 4);
    all_array_length = all_array.length - 1;   
}

var m;
function mov(cur_num) {
    m = setInterval(function () {
        var m_li = ''
        for (i = 0; i < cur_num; i++) {
            var rm = parseInt(Math.random() * all_array_length);
            m_li += "<li>" + all_array[rm] + "</li>"
        }
        $("#roll_num")[0].innerHTML = m_li;
    }, 50)
}
function resize(cur_lev) {
    var max_height = $("#back_img").height();
    var max_width = $("#back_img").width();
        $("#roll_num,#num_award").css({
            top: max_height * 0.4,
            width: max_width * 0.39,
//            height: max_width * 0.439,
            marginLeft: 0 - max_width * 0.39 / 2,
            //            fontSize: max_width * 0.4766 * 0.175 + "px",
            fontSize: 80 + "px",
            lineHeight: max_width * 0.4766 * 0.17 + "px",
            letterSpacing: max_width * 0.4766 * 0.01 + "px"
        });
        $(".sz_award").css({
            top: max_height * 0.16,
            width: max_width * 0.25,
            //            fontSize: max_width * 0.4766 * 0.053 + "px",
            fontSize: 20 + "px",
            lineHeight: max_width * 0.4766 * 0.05 + "px",
            left: max_height * 0.015
        });
        $(".bj_award").css({
            top: max_height * 0.16,
            width: max_width * 0.25,
            //            fontSize: max_width * 0.4766 * 0.053 + "px",
            fontSize: 20 + "px",
            lineHeight: max_width * 0.4766 * 0.05 + "px",
            right: max_height * 0.015
        });
        if (cur_lev == 1) {
            $("#roll_num,#num_award").css({
                top: max_height * 0.32,
                //                fontSize: max_width * 0.4766 * 0.19 + "px",
                fontSize: 82 + "px",
                letterSpacing: 0,
                lineHeight: max_width * 0.4766 * 0.26 + "px"
            })
        }
        else if (cur_lev == 2 || cur_lev == 4) {
            $("#roll_num,#num_award").css({
                top: max_height * 0.30,
                //width: max_width * 0.2,
                //height: max_width * 0.439,
                //marginLeft: 0 - max_width * 0.39 / 2,
                fontSize: 82 + "px",
                lineHeight: 60+ "px",
                letterSpacing: max_width * 0.4766 * 0.01 + "px",

            })
        }
        else if (cur_lev == 0) {
            $("#roll_num,#num_award").css({
                top: max_height * 0.38,
                width: max_width * 0.7,
                marginLeft: 0 - max_width * 0.7 / 2,
                //            fontSize: max_width * 0.4766 * 0.328 + "px",
                fontSize: 80 + "px",
                lineHeight: max_width * 0.4766 * 0.328 + "px",
                letterSpacing: "-5px"
            })
        } else if (cur_lev == 3) {//三等奖
            $("#roll_num,#num_award").css({
                top: max_height * 0.42,
                fontSize: 150 + "px",
                letterSpacing: 0,
                lineHeight: max_width * 0.4766 * 0.26 + "px"
            })
        }
}
//开场动画
function stage() {
    creat_rd_arry(); //Default.js
    all_sn();
    show_award(cur_lev, batch);
    $("#stage_left02").animate({ left: "-50%" }, 800);
    $("#stage_left01").animate({ left: "-16.85%" }, 1000);
    $("#stage_right02").animate({ right: "-50%" }, 800);
    $("#stage_right01").animate({ right: "-16.85%" }, 1000);
    $("#stage_top").animate({ top: "-16%" }, 1000);
    $("#black_mask").fadeOut(1000, function () { $("#stage").remove(); });
    mov(cur_num);
}


//开场动画,三等奖调0到9数字
function stageNumber() {
    creat_rd_arry(); //Default.js
    all_sn();
    show_award(cur_lev, batch);
    $("#stage_left02").animate({ left: "-50%" }, 800);
    $("#stage_left01").animate({ left: "-16.85%" }, 1000);
    $("#stage_right02").animate({ right: "-50%" }, 800);
    $("#stage_right01").animate({ right: "-16.85%" }, 1000);
    $("#stage_top").animate({ top: "-16%" }, 1000);
    $("#black_mask").fadeOut(1000, function () { $("#stage").remove(); });
    movNumber(cur_num);
}

var n;
function movNumber(cur_num) {
    cur_num = 1;
    n = setInterval(function () {
        var m_li = ''
        for (i = 0; i < cur_num; i++) {
            var rm = parseInt(Math.floor(Math.random() * 9) + 0);
            m_li += "<li>" + rm + "</li>"
        }
        $("#roll_num")[0].innerHTML = m_li;
    }, 50)
}

function startNumber() {
    $("#roll_num").css("display", "block");

    $("#num_award").css("display", "none").empty();
    movNumber(cur_num); //Default.js
}