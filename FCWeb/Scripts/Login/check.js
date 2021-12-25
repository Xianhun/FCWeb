//手机号码验证
function checkPhone({phone, obj}) {
    let flag = false
    if (!phone) {
        obj.html('手机号码不能为空')
        flag = false
    } else {
        if (!(/^1[3456789]\d{9}$/.test(phone))) {
            flag = false
            obj.html('手机号码格式错误')
        } else {
            //验证通过
            obj.html('')
            flag = true
        }
    }
    return flag
}

//密码验证
function checkPwd({obj1, pwd1, obj2= {}, pwd2=''}) {
    //验证密码为6-24位的数字或字母
    var str=/^[0-9a-zA-Z]{6,24}$/g;
    let flag = false
    if (pwd2 == '') {
        if (pwd1=='') {
            obj1.html('密码不能为空')
            flag = false
        } else if (str.test(pwd1)){
            flag = true
            obj1.html('')
        }else {
            obj1.html('密码只能为6-24位的数字和字母')
            flag = false
        }
    }else {
        if (!pwd2) {
            obj1.html('确认密码不能为空')
            flag = false
        } else if (pwd1 === pwd2) {
            //验证通过
            obj2.html('')
            flag = true
        } else {
            flag = false
            obj2.html('两次密码不一致！')
        }

    }
    return flag
}