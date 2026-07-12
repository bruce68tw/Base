import $ from "jquery";
import "bootstrap";
import moment from "moment";
import Mustache from "mustache";
import { Chart, registerables } from "chart.js";

Chart.register(...registerables);
(window as any).$ = $;
(window as any).jQuery = $;
(window as any).moment = moment;
(window as any).Mustache = Mustache;
(window as any).Chart = Chart;