﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using CM.VO;

namespace Caminho_Maximo1
{
    class BLL
    {

        public Double LongestCable(Graph gr, int raiz = -1)
        {
            // maximum length of cable among the connected
            // cities
            Double max_len = global.MIN_VALUE;
            Double curr_len = global.MIN_VALUE;
            // call DFS for each city to find maximum
            // length of cable
            if (raiz == -1)
            {
                foreach (Node nd in gr.nodes)
                {

                    //Console.WriteLine("novo " + nd.id);
                    // Call DFS for src vertex nd
                    curr_len = DFS(gr, nd);

                    max_len = (curr_len > max_len ? curr_len : max_len);
                    //clean paths 
                    foreach (Node no in gr.nodes)
                    {
                        no.path = new List<int>();
                        no.pathValue = 0;
                    }
                }
            }
            else
            {
                Node nd = gr.nodes.Select(x => x).Where(x => x.id == raiz).First();
                max_len = DFS(gr, nd);

                //max_len = (curr_len > max_len ? curr_len : max_len);
                //clean paths 
                foreach (Node no in gr.nodes)
                {
                    no.path = new List<int>();
                    no.pathValue = 0;
                }

            }

            return max_len;
        }



        // src is starting node for DFS traversal
        public Double DFS(Graph gr, Node src)
        {

            src.path.Add(src.id);

            Double curr_len = src.pathValue + src.value;
            Double Max_len = curr_len;
            Double Node_len = global.MIN_VALUE;

            // Traverse all adjacent
            foreach (int i in src.near.Keys)
            {

                // If node or city is not visited
                if (!src.path.Contains(i))
                {
                    // Adjacent element
                    Node adjacent = gr.nodes.Select(x => x).Where(x => x.id == i).FirstOrDefault();
                    // Total length of cable from src city
                    // to its adjacent
                    adjacent.path = src.path.ToList();
                    adjacent.pathValue = curr_len + src.near[i];
                    // Call DFS for adjacent city
                    Node_len = DFS(gr, adjacent);

                    Max_len = (Node_len > Max_len ? Node_len : Max_len);
                }

            }
            //Console.WriteLine("path id = "+ src.id );
            return Max_len;
        }

    }

    public class Grafos : Graph
    {
        #region public
        public List<List<int>> Cicles()
        {
            List<List<int>> ammount = new List<List<int>>();
            foreach (Node nd in this.nodes)
            {
                IsCiclic(this, nd);
            }
            ammount = ammount.Select(x => x).Distinct().ToList();
            return ammount;
        }

        public int Clean()
        {
            bool limpou = false;
            int limpeza = 0;
            List<int> fol = new List<int>();
#region  transforma galho em folha
            fol = this.Folhas();
            Console.WriteLine("Folhas: " + fol.Count);

            foreach (int f in fol)
            {
                Double val = global.MIN_VALUE;
                List<int> path = GalhoMaximo(f, ref val);
                if (path.Count > 2 && this.nodes.Select(x => x).Where(x => x.id == path.Last()).First().near.Count > 1)
                {
                    Node newND = new Node() { id = this.nodes.Max(x => x.id) + 1, value = 0 };
                    newND.near.Add(path.Last(), val);
                    //Retirando a ligação entre o galho e o grafo
                    this.nodes.Select(x => x).Where(x => x.id == path.Last()).First().near.Remove(path[path.Count - 2]);
                    this.nodes.Select(x => x).Where(x => x.id == path[path.Count - 2]).First().near.Remove(path.Last());

                    this.nodes.Select(x => x).Where(x => x.id == path.Last()).First().near.Add(newND.id, val);

                    //path.Remove(path.Last());
                    //path.ForEach(x => this.nodes.Remove(this.nodes.Select(y => y).Where(y => y.id == x).First()));
                    this.nodes.Add(newND);
                    limpeza += path.Count - 2;
                    limpou = true;
                }
            }
            fol.Clear();
#endregion


            #region  caso duas folhas no mesmo nó
            fol = this.Folhas();
            List<Node> NdFolList = new List<Node>();

            fol.ForEach(x => NdFolList.Add(this.nodes.Select(y => y).Where(y => y.id == x).First()));
            List<int> old = new List<int>();
            foreach (Node NdFol in NdFolList.ToList())
            {
                if (NdFol.near.Count == 0) { continue; }
                int pai = NdFol.near.Keys.First();
//#if Debug
                //Console.WriteLine("Pai: " + pai);
//#endif
                if (!old.Contains(pai))
                {
                    old.Add(NdFol.near.Keys.First());

                    List<Node> vizinhos = new List<Node>();
                    vizinhos = NdFolList.Select(x => x).Where(x => x.near.Keys.Contains(pai)).ToList();
                    Node paiND = this.nodes.Select(x => x).Where(x => x.id == pai).First();
                    if (vizinhos.Count <= 1 || paiND.near.Count == vizinhos.Count) { continue; }

                    vizinhos.ForEach(x => x.pathValue = x.near[pai] + x.value);
                    Double max = vizinhos.Select(x => x).Max(x => x.pathValue);
                    
                    //removo as ligações das folhas aos pais
                    vizinhos.ForEach(k =>
                        this.nodes.Select(x => x)
                            .Where(x => x.id == NdFol.near.Keys.First())
                            .First().near.Remove(k.id)                       
                        );
                    vizinhos.ForEach(k =>
                    this.nodes.Select(x => x).Where(x => x.id == k.id).First().near.Remove(pai)
                    );
                    //adiciono nova folha
                    Node newND = new Node() { id = this.nodes.Max(x => x.id) + 1, value = 0 };
                    newND.near.Add(pai, max);
                    this.nodes.Select(x => x)
                    .Where(x => x.id == pai)
                    .First()
                    .near.Add(newND.id, max);

                    this.nodes.Add(newND);
                    limpeza += vizinhos.Count - 1;
                    limpou = true;
                }
            }
#endregion
            Console.WriteLine("Limpou: " + limpeza);

#region Reduz caminhos
            List<List<Node>> caminhos = Caminhos();

            foreach (List<Node> caminho in caminhos)
            {
                List<int> caminhoInt = caminho.Select(x => x.id).ToList();
                List<Node> pontas = caminho.Select(x => x).Where(x => x.ponta).ToList();
                List<int> externos = new List<int>();


                if (pontas.Count == 1)
                {
                    int vizinhoA = pontas.First().near.First().Key;
                    this.nodes.Select(y => y).Where(y => y.id == vizinhoA).First().near.Remove(pontas.First().id);
                    int vizinhoB = pontas.First().near.Last().Key;
                    this.nodes.Select(y => y).Where(y => y.id == vizinhoB).First().near.Remove(pontas.First().id);
                    caminho.Remove(pontas.First());
                    Double maxA = CaminhoMaximo(caminho, this.nodes.Select(y => y).Where(y => y.id == vizinhoA).First());
                    Double maxB = CaminhoMaximo(caminho, this.nodes.Select(y => y).Where(y => y.id == vizinhoB).First());

                    Node newND = new Node() { id = this.nodes.Max(x => x.id) + 2, value = 0 };
                    newND.near.Add(pontas.First().id, (maxA > maxB ? maxA : maxB));

                    this.nodes.Select(x => x).Where(x => x.id == pontas.First().id).First().near.Clear();
                    this.nodes.Select(x => x).Where(x => x.id == pontas.First().id).First().near.Add(newND.id, (maxA > maxB ? maxA : maxB));
                    this.nodes.Add(newND);

                    limpeza += 1;
                    limpou = true;
                    continue;
                }
                else if (pontas.Count > 1)
                {

                    Node newNDLigacao = CaminhoTotal(caminho);

                    foreach (Node ponta in pontas)
                    {
                        Double max = CaminhoMaximo(caminho, ponta);

                        int externo = ponta.near.Keys.Select(x => x).Where(x => !caminhoInt.Contains(x)).First();
                        externos.Add(externo);
                        Node newND = new Node() { id = this.nodes.Max(x => x.id) + 2, value = 0 };
                        newND.near.Add(externo, max);
                        //remove ligacao
                        this.nodes.Select(y => y).Where(y => y.id == externo).First().near.Remove(ponta.id);
                        this.nodes.Select(y => y).Where(y => y.id == ponta.id).First().near.Remove(externo);
                        //adiciona node
                        this.nodes.Select(x => x).Where(x => x.id == externo).First().near.Add(newND.id, max);
                        this.nodes.Add(newND);

                    }
                    //adicionando a ligação

                    newNDLigacao.near.Keys.ToList().ForEach(x => this.nodes.Select(y => y).Where(y => y.id == x).First().near.Add(newNDLigacao.id, newNDLigacao.near[x]));
                    this.nodes.Add(newNDLigacao);

                    limpeza += caminho.Count - 1;
                    limpou = true;
                }
            }
            #endregion


#region corta nos que ligam mesmos pontos.

            List<List<Node>> duplicatas = Duplicatas();

            foreach (List<Node> par in duplicatas)
            {
                int A = par.First().near.Keys.First();
                int B = par.First().near.Keys.Last();
                int A_Max = DuplicataMaximaUnilateral(par, A);
                int B_Max = DuplicataMaximaUnilateral(par, B);
                int Max = DuplicataMaxima(par);
                foreach (Node no in par)
                {
                    if (no.id != A_Max && no.id != Max)
                    {
                        //removo ligacao ao A
                        this.nodes.Select(x => x).Where(x => x.id == no.id).First().near.Remove(A);
                        this.nodes.Select(x => x).Where(x => x.id == A).First().near.Remove(no.id);
                    }
                    if (no.id != B_Max && no.id != Max)
                    {
                        //removo ligacao ao B
                        this.nodes.Select(x => x).Where(x => x.id == no.id).First().near.Remove(B);
                        this.nodes.Select(x => x).Where(x => x.id == B).First().near.Remove(no.id);
                    }
                }
                limpeza += par.Count - 1;
                limpou = true;
            }

#endregion

            if (limpou) { limpeza += this.Clean(); }
            this.nodes.ForEach(x => x.pathValue = 0);

            this.nodes
                .Select(x => x)
                .Where(x => x.value == 0 && x.near.Count == 0)
                .ToList()
                .ForEach(y => this.nodes.Remove(y));
            return limpeza;

        }

        public int QuantosCaminhos() { return this.Caminhos().Count; }


        #endregion


        #region private
        private List<List<int>> IsCiclic(Graph gr, Node src)
        {
            List<List<int>> ammount = new List<List<int>>();
            src.path.Add(src.id);
            if (src.near.Count > 0)
            {
                foreach (int i in src.near.Keys)
                {
                    if (!src.path.Contains(i))
                    {
                        Node nd = gr.nodes.Select(x => x).Where(x => x.id == i).First();
                        if (nd.near.Count > 1)
                        {
                            nd.path = src.path.ToList();
                            ammount.Concat(IsCiclic(gr, nd));
                        }
                    }
                    else
                    {
                        src.path.Add(i);
                        ammount.Add(src.path);
                    }
                }
            }
            return ammount;
        }

        private List<int> Folhas()
        {
            return this.nodes.Select(x => x).Where(x => x.near.Count == 1).Select(x => x.id).ToList();
        }

        private Double CaminhoMaximo(List<Node> caminho, Node raiz)
        {
            Double maximo = raiz.value;
            Double atual = maximo;
            List<int> caminhoInt = caminho.Select(x => x.id).ToList();
            List<int> old = new List<int>();
            old.Add(raiz.id);
            Node nd = raiz;
            while (old.Count < caminho.Count )
            {
                int prox = nd.near.Keys.Select(x => x).Where(x => !old.Contains(x) && caminhoInt.Contains(x)).First();
                if (prox == 0) { continue; }
                atual += nd.near[(int)prox];
                nd = caminho.Select(x => x).Where(x => x.id == prox).First();
                atual += nd.value;
                maximo = (atual > maximo ? atual : maximo);
                old.Add(prox);
            }
            return maximo;
        }

        private int DuplicataMaximaUnilateral(List<Node> duplicata, int raiz)
        {
            Double maximo = global.MIN_VALUE;
            Double atual = maximo;
            int retorno = -1;
            foreach(Node nd in duplicata)
            {
                atual = nd.near[raiz] + nd.value;
                retorno = (atual > maximo ? nd.id : retorno);
                maximo = (atual > maximo ? atual : maximo);
            }
            return retorno;
        }
    
        private int DuplicataMaxima(List<Node> duplicata)
        {
            Double maximo = global.MIN_VALUE;
            Double atual = maximo;
            int retorno = -1;
            foreach (Node nd in duplicata)
            {
                atual = nd.near.First().Value + nd.near.Last().Value + nd.value;
                retorno = (atual > maximo ? nd.id : retorno);
                maximo = (atual > maximo ? atual : maximo);
            }
            return retorno;
        }

        private Node CaminhoTotal(List<Node> caminho)
        {
            Node raiz = caminho.Select(x => x).Where(x => x.ponta).First();
            Double maximo = raiz.value;
            Double atual = maximo;
            List<int> caminhoInt = caminho.Select(x => x.id).ToList();
            List<int> old = new List<int>();
            old.Add(raiz.id);
            Node nd = raiz;
            while (old.Count < caminho.Count)
            {
                int prox = nd.near.Keys.Select(x => x).Where(x => !old.Contains(x) && caminhoInt.Contains(x)).First();
                atual += nd.near[prox];
                nd = caminho.Select(x => x).Where(x => x.id == prox).First();
                atual += nd.value;
                old.Add(nd.id);
                //maximo = (atual > maximo ? atual : maximo);
            }
            //adicionando os caminhos ate os vertices
            Node retorno = new Node() { id = this.nodes.Select(x => x.id).Max(y => y) + 1 };
            retorno.value = atual;
            int externo = raiz.near.Keys.Select(x => x).Where(x => !caminhoInt.Contains(x)).First();
            retorno.near.Add(externo, raiz.near[externo]);
            externo = nd.near.Keys.Select(x => x).Where(x => !caminhoInt.Contains(x)).First();
            retorno.near.Add(externo, nd.near[externo]);

            return retorno;
        }


        private List<List<Node>> Caminhos()
        {
            Boolean mudou = true;
            List<Node> Duplas = this.nodes.Select(x => x).Where(x => x.near.Count == 2).ToList();
            List<int> old = new List<int>();

            List<List<Node>> retorno = new List<List<Node>>();
            foreach (Node no in Duplas)
            {
                List<Node> caminho = new List<Node>();
                List<int> caminhoInt = new List<int>();
                mudou = true;
                if (old.Contains(no.id)) { continue; }
                old.Add(no.id);
                caminho.Add(no);
                caminhoInt.Add(no.id);
                while (mudou)
                {
                    List<int> pontas = new List<int>();
                    foreach (Node elementoCaminho in caminho)
                    {
                        if (!caminhoInt.Contains(elementoCaminho.near.Keys.ToList()[0])) { pontas.Add(elementoCaminho.id); }

                        else if (!caminhoInt.Contains(elementoCaminho.near.Keys.ToList()[1])) { pontas.Add(elementoCaminho.id); }
                    }
                    
                    mudou = false;
                    foreach (int pont in pontas)
                    {
                        List<Node> vizinhos = Duplas.Select(x => x)
                                                    .Where(x => x.near.Keys.Contains(pont) && !old.Contains(x.id))
                                                    .ToList();
                        if (vizinhos.Count == 0) { continue; }
                        mudou = true;

                        vizinhos.ForEach(x => caminho.Add(x));
                        vizinhos.ForEach(x => old.Add(x.id));
                        vizinhos.ForEach(x => caminhoInt.Add(x.id));
                    }

                    
                    if (!mudou && caminho.Count > 1)
                    {
                        caminho.ForEach(x => x.ponta = false);
                        foreach (int pont in pontas)
                        {
                            caminho.Select(x => x).Where(x => x.id == pont).First().ponta = true;
                        }
                    }
                    if (!mudou && pontas.Count == 0 && caminho.Last().near.Keys.ToList().Contains(caminho.First().id))
                    {
                        caminho.ForEach(x => x.ponta = false);
                        caminho.First().ponta = true;
                    }
                }
                List<Node> pontas2 = new List<Node>();
                pontas2 = caminho.Select(x => x).Where(x => x.ponta).ToList();
                foreach (Node ponta in pontas2)
                {
                    Node nd = this.nodes.Select(x => x).Where(x => x.near.Keys.Contains(ponta.id)).First();
                    if (nd.near.Count == 1)
                    {
                        caminho.Clear();
                    }
                }
                if(caminho.Count > 1)
                    retorno.Add(caminho);
            }

            return retorno;
        }

        private List<List<Node>> Duplicatas()
        {
            List<Node> Duplas = this.nodes.Select(x => x).Where(x => x.near.Count == 2).ToList();
            List<List<Node>> retorno = new List<List<Node>>();
            List<int> old = new List<int>();
            foreach (Node no in Duplas)
            {
                if (old.Contains(no.id)) { continue; }
                List<Node> Pares = new List<Node>();
                Pares = Duplas.Select(x => x).Where(x => no.near.Keys.Contains(x.near.Keys.First()) && no.near.Keys.Contains(x.near.Keys.Last())).ToList();
                Pares.ForEach(x => old.Add(x.id));

                if (Pares.Count > 1)
                    retorno.Add(Pares);
            }

            return retorno;

        }





        private List<Node> Soltos()
        {
            return this.nodes.Select(x => x).Where(x => x.near.Count == 0).ToList();
        }

        private List<int> GalhoMaximo(int first, ref Double val)
        {
            Node nd, src;
            src = this.nodes.Select(x => x).Where(x => x.id == first).First();
            List<int> path = new List<int>();
            Double maxVal = global.MIN_VALUE;
            Double atualVal = global.MIN_VALUE;
            path.Add(src.id);
            nd = this.nodes.Select(x => x).Where(x => x.id == src.near.First().Key).First();
            path.Add(nd.id);
            while (nd.near.Count == 2)
            {
                nd = this.nodes.Select(x => x).Where(x => !path.Contains(x.id) && nd.near.Keys.Contains(x.id)).First();
                path.Add(nd.id);
            }

            //nd é o primeiro nó fora do galho
            //maxVal = atualVal = nd.value;

            //agora nd vai ser o ultimo no do galho
            nd = this.nodes.Select(x => x).Where(x => x.id == path[path.Count - 2]).First();

            int i = 2;
            maxVal = atualVal = nd.near[path[path.Count - 1]] + nd.value;
            while (nd.near.Count == 2)
            {
                i++;
                nd = this.nodes.Select(x => x).Where(x => x.id == path[path.Count - i]).First();

                atualVal += nd.near[path[path.Count - i + 1]];
                atualVal += nd.value;
                maxVal = (atualVal > maxVal ? atualVal : maxVal);
            }
            val = maxVal;
            return path;
        }
        #endregion
    }

}
