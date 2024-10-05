window.env = {
    API_BASE_URL: 'http://localhost:5067',
    PROGRAM_NO: "trans_inform"
}

async function postData(json, api) {
    const res = await fetch(api, {
        method: "POST",
        headers: {
            'content-type': 'application/json'
        },
        body: JSON.stringify(json)
    })
    return res;
}

function sendAPI() {
    var FileName;
    var user_id = "";
    const host = window.env.API_BASE_URL;
    const program_no = window.env.PROGRAM_NO;
    const parsed = new URLSearchParams(window.location.search);

    postData({
        WebPid1: parsed.get("WebPid1"),
        program_no: program_no
    }, `${host}/api/pub/check`).then(function (response) {
        return response.json()
    }).then(function (data) {
        if (data.ident) {
            user_id = data.msg;
            postData({Id: user_id}, `${host}/api/pub/acad`).then(function (response) {
                return response.json()
            }).then(function (data) {
                postData({Acadno: data.id}, `${host}/api/pub/date`)
                    .then(function (response) {
                        if (response.status === 204)
                            throw "不在時限內";
                        return response.json()
                    }).then(function (data) {
                    var json = {}
                    json.syearEnd = data.syearEnd;
                    json.semEnd = data.semEnd;
                    json.StudentId = user_id;
                    postData(json, `${host}/api/Calculate`)
                        .then(function (response) {
                            return response.json();
                        }).then(function (data) {
                        if (data)
                            return postData(json, `${host}/api/generate/inform`);
                        // eslint-disable-next-line no-throw-literal
                        else throw "沒有資料";
                    }).then(function (response) {
                        var FileNameEncode = response.headers.get('Content-Disposition').split('filename=')[1];
                        FileName = decodeURIComponent(FileNameEncode);
                        return response.blob();
                    }).then(function (blob) {
                        var url = window.URL.createObjectURL(blob);
                        var a = document.createElement('a');
                        a.href = url;
                        a.download = FileName;
                        document.body.appendChild(a);
                        a.click();
                        window.URL.revokeObjectURL(url);
                        document.body.removeChild(a);
                    }).catch(function (Error) {
                        alert(Error);
                    });
                }).catch(function (Error) {
                    alert(Error);
                });
            }).catch(function (Error) {
                alert(Error);
            });
        } else {
            alert(data.msg)
        }
    }).catch(function (Error) {
        console.log(Error)
    })


}
