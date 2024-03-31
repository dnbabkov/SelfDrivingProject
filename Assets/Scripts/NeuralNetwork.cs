using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork
{
    [SerializeField] private int hiddenLayersCount = 1;
    [SerializeField] private int hiddenLayerNeuronCount = 5;
    [SerializeField] private int outputNeurons = 1;
    [SerializeField] private int inputNeurons = 9;
    [SerializeField] private float maxInitialValue = 1f;

    private const float EULER_NUMBER = 2.71828f;
    private List<List<float>> neurons;
    private List<float[][]> weights;
    private int totalLayerCount = 0;

    public NeuralNetwork() {
        totalLayerCount = hiddenLayersCount + 2;

        // weights and neurons initialization
        weights = new List<float[][]>();
        neurons = new List<List<float>>();

        // filling the neurons
        for (int i = 0; i<totalLayerCount; i++) {
            float[][] layerWeights;
            List<float> layer = new List<float>();
            int layerSize = getLayerSize(i);
            if (i != totalLayerCount - 1) {
                layerWeights = new float[layerSize][];
                int nextLayerSize = getLayerSize(i + 1);
                for (int j = 0; j < layerSize; j++) {
                    layerWeights[j] = new float[nextLayerSize];
                    layer.Add(0);
                    for (int k = 0; k < nextLayerSize; k++) {
                        layerWeights[j][k] = randomValue();
                    }
                }
                weights.Add(layerWeights);
                neurons.Add(layer);
            }
        }
    }

    public void NeuralNetworkUpdate(float neuralNetReturn, float prevNeuralNetReturn, float eTrace, float prevReward) {
    }

    public float getGradient() {
        return 0;
    }

    public void feedForward(float[] inputs) {
        // setting inputs in the input layer
        List<float> inputLayer = neurons[0];

        for (int i = 0; i < inputs.Length; i++) {
            inputLayer[i] = inputs[i];
        }

        // updating neurons between input and output layers
        for (int layer = 0; layer < neurons.Count-2; layer++) {
            float[][] layerWeights = weights[layer];
            List<float> layerNeurons = neurons[layer];
            List<float> nextLayerNeurons = neurons[layer + 1];
            for (int i = 0; i<nextLayerNeurons.Count-1; i++) {
                float sum = 0;
                for (int j = 0; j < layerNeurons.Count-1; j++) {
                    sum += layerWeights[j][i] * layerNeurons[j]; // feed forward mult
                }
                nextLayerNeurons[i] = sigmoid(sum);
            }
        }
        float[][] lastLayerWeights = weights[neurons.Count - 2];
        List<float> secondToLastLayerNeurons = neurons[neurons.Count - 2];
        List<float> outputLayerNeurons = neurons[neurons.Count-1];
        for (int i = 0; i < outputLayerNeurons.Count-1; i++) {
            float sum = 0f;
            for (int j = 0; j < secondToLastLayerNeurons.Count - 1; j++) {
                sum += lastLayerWeights[j][i] * secondToLastLayerNeurons[j];
            }
            outputLayerNeurons[i] = sum;
        }
        return;
    }

    public float sigmoid(float x) {
        return (1 / (float)(1 + Mathf.Pow(EULER_NUMBER, -x)));
    }

    public float randomValue() {
        return Random.Range(-maxInitialValue, maxInitialValue);
    }

    public int getLayerSize(int i) {
        if (i == 0)
        {
            return inputNeurons;
        }
        else if(i== hiddenLayersCount + 1) {
            return outputNeurons;
        }
        else {
            return hiddenLayerNeuronCount;
        }
    }

    public List<List<float>> getNeurons() {
        return neurons;
    }
    
    public List<float[][]> getWeights() {
        return weights;
    }

    public List<float> getOutput() {
        return neurons[neurons.Count - 1];
    }
}