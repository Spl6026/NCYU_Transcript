import React, {useState, useEffect} from "react";
import './App.css';
import Datalist from "./components/datalist";
import Select from "./components/select";

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

const App = ({user_id, program_no}) => {
    var chineseNumber = ("一二三四五六七八九").split('');
    var chineseClass = ("甲乙丙丁戊己庚辛壬癸子丑寅卯").split('');
    const [DeptData, setDeptData] = useState([]);
    const [SecData, setSecData] = useState([]);
    const [GradeData] = useState(() => {
        var json = []
        for (var i = 0; i < 5; i++) {
            json.push({"Id": i + 1, "Name": chineseNumber[i]})
        }
        return json
    });

    const [ClassData] = useState(() => {
        var json = []
        for (var i = 0; i < 14; i++) {
            json.push({"Id": i + 1, "Name": chineseClass[i]})
        }
        return json
    });

    const [SyearData] = useState(() => {
        const currentYear = new Date().getFullYear() - 1911;
        const years = Array.from({length: 20}, (_, index) => currentYear - index);
        var json = []
        for (const year of years) {
            json.push({"Id": year, "Name": year})
        }
        return json
    });

    const [SemData] = useState(() => {
        var json = []
        for (var i = 1; i <= 3; i++) {
            json.push({"Id": i, "Name": i})
        }
        return json
    });

    const [StuData, setStuData] = useState([]);
    const [deptOption, setDeptOption] = useState("");
    const [secOption, setSecOption] = useState("");
    const [gradeOption, setGradeOption] = useState("");
    const [classOption, setClassOption] = useState("");
    const [syearOption, setSyearOption] = useState("");
    const [semOption, setSemOption] = useState("");
    const [stuOption, setStuOption] = useState("");
    const [isRank, setIsRank] = useState(false);
    const [isGrading, setIsGrading] = useState(false);

    function rank(e) {
        setIsRank(e.target.checked);
    }

    function grading(e) {
        setIsGrading(e.target.checked);
    }

    function sendAPI() {
        var json = {}
        var range = {};
        json.user_id = user_id;
        json.DeptId = deptOption;
        json.Secno = secOption;
        json.Grade = gradeOption;
        json.Clacod = classOption;
        json.syearEnd = syearOption;
        json.semEnd = semOption;
        json.Isrank = isRank;
        json.StudentId = stuOption;
        json.IsGrading = isGrading;

        range.user_id = user_id;
        range.program_no = program_no;
        range.data = json.DeptId;
        range.tblname = "STUFILE";
        range.clnname = "DEPTNO";
        if (deptOption === "" && stuOption === "")
            alert("系所或學號須擇一輸入")

        if (stuOption !== "") {
            json.DeptId = null;
            json.Secno = 0;
            json.Grade = 0;
            json.Clacod = 0;
        } else
            json.StudentId = null;

        var FileName;
        postData(range, "http://localhost:44329/api/pub/range")
            .then(function (response) {
                return response.json()
            }).then(function (data) {
            if (data) {
                postData(json, "http://localhost:44371/api/Calculate")
                    .then(function (response) {
                        return postData(json, "http://localhost:55082/api/generate");
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
                    console.log(Error);
                });
            } else alert("無權存取資料");
        }).catch(function (Error) {
            console.log(Error);
        });
    }

    useEffect(() => {
        var range = {}
        range.user_id = user_id;
        range.program_no = program_no;
        range.tblname = "STUFILE";
        range.clnname = "DEPTNO";
        postData(range, "http://localhost:44329/api/pub/dept")
            .then(function (res) {
                return res.json();
            }).then(function (data) {
            setDeptData(data);
        }).catch(function (Error) {
            console.log(Error)
        })
    }, [])

    useEffect(() => {
        var json = {}
        json.Id = deptOption
        postData(json, "http://localhost:44329/api/pub/sec")
            .then(function (res) {
                return res.json();
            }).then(function (data) {
            const defaultData = [{
                "Id": "0",
                "Name": "不分組"
            }];
            if (data.length === 0)
                Object.assign(data, defaultData);

            setSecData(data)
        }).catch(function (Error) {
            console.log(Error)
        })
    }, [deptOption])

    useEffect(() => {
        var json = {}
        json.DeptId = deptOption;
        json.Secno = secOption;
        json.Grade = gradeOption;
        json.Clacod = classOption;
        json.syearEnd = syearOption;
        json.semEnd = semOption;
        if (deptOption !== "" && secOption !== "" && gradeOption !== "" && classOption !== "" && syearOption !== "" && semOption !== "")
            postData(json, "http://localhost:44329/api/pub/stu")
                .then(function (res) {
                    return res.json();
                }).then(function (data) {
                setStuData(data)
            }).catch(function (Error) {
                console.log(Error)
            })
    }, [deptOption, secOption, gradeOption, classOption, syearOption, semOption])

    return (
        <div className="App">
            <div>
                <div className="img">
                    <img alt="title" src="https://wwwcms.ncyu.edu.tw/images/logo.png"/>
                </div>
                <p>系所：</p>
                <Datalist htmlId="dept_list" SelectData={DeptData} setOption={setDeptOption}/>

                <p>組別：</p>
                <Select SelectData={SecData} setOption={setSecOption}/>

                <p>年級：</p>
                <Select SelectData={GradeData} setOption={setGradeOption}/>

                <p>班級：</p>
                <Select SelectData={ClassData} setOption={setClassOption}/>

                <p>學年：</p>
                <Select SelectData={SyearData} setOption={setSyearOption}/>

                <p>學期：</p>
                <Select SelectData={SemData} setOption={setSemOption}/>

                <p>學號：</p>
                <Datalist htmlId="stu_list" SelectData={StuData} setOption={setStuOption}/>

                <p>
                    顯示排名
                    <input type="checkbox" onChange={rank}/>
                </p>

                <p>
                    用等第顯示
                    <input type="checkbox" onChange={grading}/>
                </p>

                <button className="submit" onClick={sendAPI}>送出</button>
            </div>
        </div>
    );
}

export default App;
