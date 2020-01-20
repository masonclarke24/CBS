// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.
window.addEventListener("load", addStatusMessage);

function rowClicked(row) {
    if ($(row).css("background-color") !== "rgb(204, 204, 204)" && $(row).find("input[type=radio]").length != 0) {
        $(".selected").removeClass("selected");
        $(row).addClass("selected");

        $(row).find("input[type=radio]").attr("checked", true);
    }
    var td = $(row).children().filter(".text-warning");
    var expandMe = $(td).parent().next();
    if ($(expandMe).css("display") == "table-row")
        $(expandMe).css("display", "none")
    else
        $(expandMe).css("display", "table-row")
}
function addGolfer(button) {
    var childCount = $("input[name^=Golfers]").length;
    if (childCount > 3) return;
    var formGroup = `<div class="form-group">
                    <label>Member Number</label>
                    <input class="form-control" name="Golfers[` + (childCount).toString() + `]" />
                    <span class="text-danger" \></span>
                </div>`;
    $(button).parent().append(formGroup);
}
function removeGolfer(button) {
    $("input[name^=Golfers]").last().parent().remove()
}
function addErrorMessages(errors) {
    for (var i = 0; i < errors.length; i++) {
        $("input[name=Golfers[" + i.toString() + "]").parent().children("span").text("Member does not exist");
    }
}
function selectAvaliableTime() {
    var teeTimes = $("table").children("tbody").children("tr");
    for (var i = 0; i < teeTimes.length; i++) {
        if ($(teeTimes[i]).attr("style") !== "background-color: #CCC") {
            var radioButton = $(teeTimes[i]).find("input[type=radio]");
            if (radioButton.length == 0) continue;
            radioButton.attr("checked", true);
            $(teeTimes[i]).addClass("selected");

            $("table").parent().scrollTop((($("table tr.d-sm-table-row").index($(".selected")) + 1) * $(".selected").height()));
            return;
        }

    }
}

function startDateEntered(date, callback, callbackArgs) {
    $.ajax({
        type: "POST",
        url: "/StandingTeeTimeRequests?handler=ChangeDate",
        contentType: "text/plain",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("RequestVerificationToken",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        data: date,
        success: function (response) {
            $("select[name='EndDate']").replaceWith(response);
            $("select[name='EndDate']").parents("form").attr("action", "/StandingTeeTimeRequests?handler=View")
            $("select[name='EndDate']").on("change", dateChanged);
            if (typeof callback != 'undefined')
                callback(callbackArgs);
        },
        failure: function (response) {
            console.log(response);
        }
    });
}
function selectEndDate(endDate) {
    $("option[value='" + endDate.toString() + "']").attr("selected", true);
}

function dateChanged() {
    $("form").submit();
}


function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}