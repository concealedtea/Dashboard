$(document).ready(function () {
    var d = document.getElementById("total_rca_c"), b = new Chart(d, { type: "bar", label: ["Clicks"], data: {}, options: { legend: { display: !1 }, scales: { xAxes: [{ label: ["Clicks"], ticks: { autoSkip: !1, labels: ["Clicks"] } }], yAxes: [{ label: [], position: "left", id: "A", ticks: { beginAtZero: !0 } }] } } }); (function () {
        $.ajax({
            url: "/Revenue/PastRCA", type: "get", dataType: "json", success: function (c) {
                ajaxData = c; backgroundColor = "rgb(" + Math.floor(256 * Math.random()) + "," + Math.floor(256 * Math.random()) + "," + Math.floor(256 *
                Math.random()) + ")"; for (var a = 0; a < c.Date.length; a++) b.data.datasets[a] = { label: [ajaxData.Date[a]], backgroundColor: backgroundColor, data: [ajaxData.Clicks[a]], yAxisID: "A" }; b.update()
            }, error: function () { console.log("Error") }
        })
    })()
});
